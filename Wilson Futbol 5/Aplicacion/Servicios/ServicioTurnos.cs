using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Dominio.Enums;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Contiene la logica principal para consultar y administrar turnos.
// Esta clase implementa IServicioTurnos, que es el contrato que van a usar los controladores.
public class ServicioTurnos : IServicioTurnos
{
    private readonly WilsonDbContext _contexto;

    // Recibe el DbContext por inyeccion de dependencias.
    // ASP.NET Core crea y entrega esta instancia automaticamente cuando se usa el servicio.
    public ServicioTurnos(WilsonDbContext contexto)
    {
        _contexto = contexto;
    }

    // Calcula los horarios disponibles de una fecha puntual.
    // No guarda datos: solo consulta la configuracion, los horarios de atencion y los turnos ya reservados.
    public async Task<IReadOnlyList<TurnoDisponibleDto>> ObtenerDisponibilidadPorFechaAsync(DateOnly fecha)
    {
        // Buscamos la configuracion actual del negocio.
        // De aca salen valores como duracion del turno, precio por persona y cantidad de jugadores.
        var configuracion = await _contexto.ConfiguracionesNegocio
            .AsNoTracking()
            .FirstAsync();

        // Buscamos la cancha activa.
        // Aunque hoy haya una sola cancha, el modelo queda preparado para crecer a futuro.
        var cancha = await _contexto.Canchas
            .AsNoTracking()
            .FirstAsync(cancha => cancha.Activa);

        // Buscamos el horario de atencion correspondiente al dia de la fecha solicitada.
        // Ejemplo: si la fecha cae un viernes, buscamos la configuracion de horarios para viernes.
        var horarioAtencion = await _contexto.HorariosAtencion
            .AsNoTracking()
            .FirstOrDefaultAsync(horario =>
                horario.CanchaId == cancha.Id &&
                horario.DiaSemana == fecha.DayOfWeek &&
                horario.Activo);

        // Si ese dia la cancha no atiende, no hay horarios para mostrar.
        // Por eso devolvemos una lista vacia.
        if (horarioAtencion is null)
        {
            return [];
        }

        // Convertimos la fecha recibida a un rango completo del dia.
        // Esto nos permite buscar todos los turnos reservados entre las 00:00 y las 23:59:59.
        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = fecha.ToDateTime(TimeOnly.MaxValue);

        // Traemos los turnos ya reservados para esa cancha y ese dia.
        // Solo nos interesan los turnos en estado Reservado, porque los cancelados liberan el horario.
        var turnosReservados = await _contexto.Turnos
            .AsNoTracking()
            .Where(turno =>
                turno.CanchaId == cancha.Id &&
                turno.FechaHoraInicio >= inicioDia &&
                turno.FechaHoraInicio <= finDia &&
                turno.EstadoTurno == EstadoTurno.Reservado)
            .ToListAsync();

        // La duracion se toma desde configuracion para que el dueno pueda modificarla sin cambiar codigo.
        var duracionTurno = TimeSpan.FromMinutes(configuracion.DuracionTurnoNormalMinutos);

        // El precio total se calcula como precio por persona multiplicado por la cantidad de jugadores.
        // En futbol 5 son 10 jugadores, pero tambien queda parametrizado en configuracion.
        var precioTotal = configuracion.PrecioPorPersona * configuracion.CantidadJugadoresPorTurno;

        // Aca vamos a ir armando la lista final que va a recibir el frontend.
        var horariosDisponibles = new List<TurnoDisponibleDto>();

        // Armamos el primer horario posible del dia y la hora limite de cierre.
        // Ejemplo: fecha 2026-07-15 + apertura 18:00 = 2026-07-15 18:00.
        var fechaHoraInicio = fecha.ToDateTime(horarioAtencion.HoraApertura);
        var fechaHoraCierre = fecha.ToDateTime(horarioAtencion.HoraCierre);

        // Recorremos la franja de atencion en bloques del tamano de un turno.
        // Si la cancha abre 18:00 y cierra 23:00, generamos 18-19, 19-20, 20-21, etc.
        while (fechaHoraInicio.Add(duracionTurno) <= fechaHoraCierre)
        {
            // Calculamos el final del bloque actual.
            var fechaHoraFin = fechaHoraInicio.Add(duracionTurno);

            // Verificamos si el bloque actual se pisa con algun turno reservado.
            // Esta comparacion detecta superposiciones aunque el turno reservado no empiece exactamente a la misma hora.
            var estaOcupado = turnosReservados.Any(turno =>
                fechaHoraInicio < turno.FechaHoraFin &&
                fechaHoraFin > turno.FechaHoraInicio);

            // Agregamos el bloque a la respuesta.
            // Si esta ocupado, el frontend lo va a mostrar bloqueado; si no, lo va a permitir seleccionar.
            horariosDisponibles.Add(new TurnoDisponibleDto
            {
                FechaHoraInicio = fechaHoraInicio,
                FechaHoraFin = fechaHoraFin,
                Disponible = !estaOcupado,
                PrecioTotal = precioTotal
            });

            // Avanzamos al siguiente bloque horario.
            fechaHoraInicio = fechaHoraInicio.Add(duracionTurno);
        }

        // Devolvemos todos los bloques generados para esa fecha, disponibles y ocupados.
        return horariosDisponibles;
    }
}
