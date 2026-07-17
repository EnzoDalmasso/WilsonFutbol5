using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.TurnosFijos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Dominio.Entidades;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Contiene la logica para administrar turnos fijos semanales.
public class ServicioTurnosFijos : IServicioTurnosFijos
{
    private readonly WilsonDbContext _contexto;

    // Recibe el DbContext para consultar y guardar clientes y turnos fijos.
    public ServicioTurnosFijos(WilsonDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<TurnoFijoDto>> ObtenerTurnosFijosAsync()
    {
        // Buscamos los turnos fijos con datos del cliente para mostrarlos en el panel del dueno.
        var turnosFijos = await _contexto.TurnosFijos
            .AsNoTracking()
            .OrderBy(turnoFijo => turnoFijo.DiaSemana)
            .ThenBy(turnoFijo => turnoFijo.HoraInicio)
            .Select(turnoFijo => new TurnoFijoDto
            {
                TurnoFijoId = turnoFijo.Id,
                NombreCliente = $"{turnoFijo.Cliente.Nombre} {turnoFijo.Cliente.Apellido}",
                TelefonoCliente = turnoFijo.Cliente.TelefonoCliente,
                DiaSemana = turnoFijo.DiaSemana,
                DiaSemanaTexto = ObtenerNombreDia(turnoFijo.DiaSemana),
                HoraInicio = turnoFijo.HoraInicio,
                HoraFin = turnoFijo.HoraFin,
                Activo = turnoFijo.Activo,
                FechaDesde = turnoFijo.FechaDesde,
                FechaHasta = turnoFijo.FechaHasta,
                Observacion = turnoFijo.Observacion
            })
            .ToListAsync();

        return turnosFijos;
    }


    public async Task<TurnoFijoCreadoDto> CrearTurnoFijoAsync(CrearTurnoFijoDto dto)
    {
        // Validamos datos basicos antes de guardar.
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

        if (dto.HoraInicio >= dto.HoraFin)
        {
            throw new InvalidOperationException("La hora de inicio debe ser menor a la hora de fin.");
        }

        if (dto.FechaHasta is not null && dto.FechaHasta < dto.FechaDesde)
        {
            throw new InvalidOperationException("La fecha hasta no puede ser menor que la fecha desde.");
        }

        // Por ahora usamos la cancha activa.
        // Si mas adelante hay varias canchas, el DTO deberia recibir CanchaId.
        var cancha = await _contexto.Canchas
            .FirstAsync(cancha => cancha.Activa);

        // Validamos que el turno fijo entre dentro del horario de atencion de ese dia.
        var horarioAtencion = await _contexto.HorariosAtencion
            .AsNoTracking()
            .FirstOrDefaultAsync(horario =>
                horario.CanchaId == cancha.Id &&
                horario.DiaSemana == dto.DiaSemana &&
                horario.Activo);

        if (horarioAtencion is null)
        {
            throw new InvalidOperationException("La cancha no tiene horario de atencion activo para ese dia.");
        }

        if (dto.HoraInicio < horarioAtencion.HoraApertura ||
            dto.HoraFin > horarioAtencion.HoraCierre)
        {
            throw new InvalidOperationException("El turno fijo esta fuera del horario de atencion.");
        }

        // Revisamos que no exista otro turno fijo activo que se cruce en el mismo dia.
        var existeTurnoFijoOcupado = await _contexto.TurnosFijos
            .AsNoTracking()
            .AnyAsync(turnoFijo =>
                turnoFijo.CanchaId == cancha.Id &&
                turnoFijo.Activo &&
                turnoFijo.DiaSemana == dto.DiaSemana &&
                dto.FechaDesde <= (turnoFijo.FechaHasta ?? DateOnly.MaxValue) &&
                (dto.FechaHasta ?? DateOnly.MaxValue) >= turnoFijo.FechaDesde &&
                dto.HoraInicio < turnoFijo.HoraFin &&
                dto.HoraFin > turnoFijo.HoraInicio);

        if (existeTurnoFijoOcupado)
        {
            throw new InvalidOperationException("Ya existe un turno fijo activo que se cruza con ese horario.");
        }

        var telefono = dto.TelefonoCliente.Trim();

        // Reutilizamos un cliente si ya existe con el mismo telefono.
        // Si no existe, creamos uno nuevo con los datos recibidos.
        var cliente = await _contexto.Clientes
            .FirstOrDefaultAsync(cliente => cliente.TelefonoCliente == telefono);

        if (cliente is null)
        {
            cliente = new Cliente
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                TelefonoCliente = telefono
            };

            _contexto.Clientes.Add(cliente);
        }

        var turnoFijo = new TurnoFijo
        {
            CanchaId = cancha.Id,
            Cliente = cliente,
            DiaSemana = dto.DiaSemana,
            HoraInicio = dto.HoraInicio,
            HoraFin = dto.HoraFin,
            Activo = true,
            FechaDesde = dto.FechaDesde,
            FechaHasta = dto.FechaHasta,
            Observacion = dto.Observacion?.Trim()
        };

        _contexto.TurnosFijos.Add(turnoFijo);

        await _contexto.SaveChangesAsync();

        return new TurnoFijoCreadoDto
        {
            TurnoFijoId = turnoFijo.Id,
            NombreCliente = $"{cliente.Nombre} {cliente.Apellido}",
            TelefonoCliente = cliente.TelefonoCliente,
            DiaSemana = turnoFijo.DiaSemana,
            HoraInicio = turnoFijo.HoraInicio,
            HoraFin = turnoFijo.HoraFin,
            FechaDesde = turnoFijo.FechaDesde,
            FechaHasta = turnoFijo.FechaHasta,
            Observacion = turnoFijo.Observacion,
            Mensaje = "Turno fijo creado correctamente."
        };
    }

    public async Task<TurnoFijoDto> DesactivarTurnoFijoAsync(int turnoFijoId)
    {
        // Buscamos el turno fijo con su cliente porque lo vamos a devolver actualizado al front.
        var turnoFijo = await _contexto.TurnosFijos
            .Include(turnoFijo => turnoFijo.Cliente)
            .FirstOrDefaultAsync(turnoFijo => turnoFijo.Id == turnoFijoId);

        if (turnoFijo is null)
        {
            throw new InvalidOperationException("No se encontro el turno fijo.");
        }

        if (!turnoFijo.Activo)
        {
            throw new InvalidOperationException("El turno fijo ya esta desactivado.");
        }

        // No borramos el turno fijo: lo apagamos para conservar historial.
        // Al quedar inactivo, deja de bloquear la disponibilidad.
        turnoFijo.Activo = false;

        var fechaHoy = DateOnly.FromDateTime(RelojNegocio.AhoraArgentina());

        if (turnoFijo.FechaHasta is null || turnoFijo.FechaHasta > fechaHoy)
        {
            turnoFijo.FechaHasta = fechaHoy;
        }

        await _contexto.SaveChangesAsync();

        return new TurnoFijoDto
        {
            TurnoFijoId = turnoFijo.Id,
            NombreCliente = $"{turnoFijo.Cliente.Nombre} {turnoFijo.Cliente.Apellido}",
            TelefonoCliente = turnoFijo.Cliente.TelefonoCliente,
            DiaSemana = turnoFijo.DiaSemana,
            DiaSemanaTexto = ObtenerNombreDia(turnoFijo.DiaSemana),
            HoraInicio = turnoFijo.HoraInicio,
            HoraFin = turnoFijo.HoraFin,
            Activo = turnoFijo.Activo,
            FechaDesde = turnoFijo.FechaDesde,
            FechaHasta = turnoFijo.FechaHasta,
            Observacion = turnoFijo.Observacion
        };
    }

    private static string ObtenerNombreDia(DayOfWeek diaSemana)
    {
        return diaSemana switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miercoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sabado",
            DayOfWeek.Sunday => "Domingo",
            _ => diaSemana.ToString()
        };
    }
}
