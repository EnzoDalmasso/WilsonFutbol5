using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Dominio.Enums;
using Wilson_Futbol_5.Infraestructura.Persistencia;
using Wilson_Futbol_5.Dominio.Entidades;

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

    public async Task<TurnoReservadoDto> ReservarTurnoAsync(ReservarTurnoDto dto)
    {
        // Validamos datos basicos que no conviene dejar pasar al flujo de reserva.
        // Aunque el DTO tenga atributos de validacion, esta capa protege la logica de negocio.
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new InvalidOperationException("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Apellido))
        {
            throw new InvalidOperationException("El apellido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.TelefonoCliente))
        {
            throw new InvalidOperationException("El telefono es obligatorio.");
        }

        if (dto.FechaHoraInicio == default)
        {
            throw new InvalidOperationException("La fecha y hora de inicio es obligatoria.");
        }

        if (dto.FechaHoraInicio <= DateTime.Now)
        {
            throw new InvalidOperationException("No se puede reservar un turno en una fecha pasada.");
        }

        // Buscamos la configuracion del negocio para calcular duracion y precio.
        var configuracion = await _contexto.ConfiguracionesNegocio
            .FirstAsync();

        // Buscamos la cancha activa sobre la que se va a crear el turno.
        var cancha = await _contexto.Canchas
            .FirstAsync(cancha => cancha.Activa);

        // La duracion depende del tipo de turno elegido.
        // Un turno normal dura 60 minutos, mientras que un cumpleanios usa la duracion configurable.
        var duracionMinutos = dto.TipoTurno == TipoTurno.Cumpleanios
            ? configuracion.DuracionCumpleaniosMinutos
            : configuracion.DuracionTurnoNormalMinutos;

        var fechaHoraFin = dto.FechaHoraInicio.AddMinutes(duracionMinutos);

        // Validamos que exista horario de atencion para el dia solicitado.
        var horarioAtencion = await _contexto.HorariosAtencion
            .FirstOrDefaultAsync(horario =>
                horario.CanchaId == cancha.Id &&
                horario.DiaSemana == dto.FechaHoraInicio.DayOfWeek &&
                horario.Activo);

        if (horarioAtencion is null)
        {
            throw new InvalidOperationException("La cancha no atiende en el dia solicitado.");
        }

        // Convertimos apertura y cierre a DateTime para compararlos contra el horario pedido.
        var fechaTurno = DateOnly.FromDateTime(dto.FechaHoraInicio);
        var fechaHoraApertura = fechaTurno.ToDateTime(horarioAtencion.HoraApertura);
        var fechaHoraCierre = fechaTurno.ToDateTime(horarioAtencion.HoraCierre);

        // Validamos que el turno completo entre dentro del horario de atencion.
        if (dto.FechaHoraInicio < fechaHoraApertura || fechaHoraFin > fechaHoraCierre)
        {
            throw new InvalidOperationException("El turno solicitado esta fuera del horario de atencion.");
        }

        // Buscamos si ya existe un turno reservado que se pise con el horario solicitado.
        var existeTurnoOcupado = await _contexto.Turnos
            .AnyAsync(turno =>
                turno.CanchaId == cancha.Id &&
                turno.EstadoTurno == EstadoTurno.Reservado &&
                dto.FechaHoraInicio < turno.FechaHoraFin &&
                fechaHoraFin > turno.FechaHoraInicio);

        if (existeTurnoOcupado)
        {
            throw new InvalidOperationException("El horario solicitado ya esta reservado.");
        }

        // Calculamos el precio total congelado al momento de reservar.
        // Si el dueno cambia el precio despues, este turno mantiene el valor original.
        var precioTotal = configuracion.PrecioPorPersona * configuracion.CantidadJugadoresPorTurno;

        // Creamos el cliente con los datos recibidos desde el formulario.
        // Mas adelante podemos mejorar esto para reutilizar clientes por telefono.
        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            TelefonoCliente = dto.TelefonoCliente.Trim()
        };

        var turno = new Turno
        {
            CanchaId = cancha.Id,
            Cliente = cliente,
            FechaHoraInicio = dto.FechaHoraInicio,
            FechaHoraFin = fechaHoraFin,
            TipoTurno = dto.TipoTurno,
            EstadoTurno = EstadoTurno.Reservado,
            PrecioPorPersonaAlReservar = configuracion.PrecioPorPersona,
            CantidadJugadores = configuracion.CantidadJugadoresPorTurno,
            PrecioTotal = precioTotal
        };

        // Creamos notificaciones pendientes.
        // Todavia no enviamos WhatsApp real: solo dejamos los mensajes listos para un servicio posterior.
        turno.NotificacionesWhatsApp.Add(new NotificacionWhatsApp
        {
            TelefonoDestino = cliente.TelefonoCliente,
            TipoNotificacion = TipoNotificacionWhatsApp.ConfirmacionCliente,
            Mensaje = $"Turno confirmado para el {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}.",
            FechaProgramada = DateTime.UtcNow
        });

        turno.NotificacionesWhatsApp.Add(new NotificacionWhatsApp
        {
            TelefonoDestino = cliente.TelefonoCliente,
            TipoNotificacion = TipoNotificacionWhatsApp.RecordatorioCliente,
            Mensaje = $"Recordatorio: tenes turno el {turno.FechaHoraInicio:dd/MM/yyyy HH:mm}.",
            FechaProgramada = turno.FechaHoraInicio.AddHours(-configuracion.HorasAnticipacionRecordatorio)
        });

        _contexto.Turnos.Add(turno);

        // Guardamos en una sola operacion el cliente, el turno y sus notificaciones.
        await _contexto.SaveChangesAsync();

        return new TurnoReservadoDto
        {
            TurnoId = turno.Id,
            NombreCliente = $"{cliente.Nombre} {cliente.Apellido}",
            TelefonoCliente = cliente.TelefonoCliente,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaHoraFin = turno.FechaHoraFin,
            PrecioTotal = turno.PrecioTotal,
            TokenCancelacion = turno.TokenCancelacion
        };
    }
}
