using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Dominio.Entidades;
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

    public async Task<IReadOnlyList<TurnoPendienteConfirmacionDto>> ObtenerTurnosPendientesConfirmacionAsync()
    {
        // Buscamos los turnos que todavia esperan aprobacion del dueno.
        // Incluimos los datos del cliente porque el dueno necesita ver quien hizo la reserva.
        var turnosPendientes = await _contexto.Turnos
            .AsNoTracking()
            .Where(turno => turno.EstadoTurno == EstadoTurno.EnEsperaDePago)
            .OrderBy(turno => turno.FechaHoraInicio)
            .Select(turno => new TurnoPendienteConfirmacionDto
            {
                TurnoId = turno.Id,
                NombreCliente = $"{turno.Cliente.Nombre} {turno.Cliente.Apellido}",
                TelefonoCliente = turno.Cliente.TelefonoCliente,
                FechaHoraInicio = turno.FechaHoraInicio,
                FechaHoraFin = turno.FechaHoraFin,
                PrecioTotal = turno.PrecioTotal,
                MontoSena = turno.MontoSena,
                FechaVencimientoReserva = turno.FechaVencimientoReserva,
                EstadoTurno = turno.EstadoTurno.ToString(),
                TextoEstado = "Pendiente del dueño"

            })
            .ToListAsync();

        return turnosPendientes;
    }

    public async Task<IReadOnlyList<ReservaEspecialDto>> ObtenerReservasEspecialesAsync()
    {
        var fechaActual = DateTime.Now.Date;

        // Las reservas especiales se guardan como turnos de tipo Cumpleanios.
        // Filtramos desde hoy para que el panel muestre lo que todavia sirve operar.
        var reservasEspeciales = await _contexto.Turnos
            .AsNoTracking()
            .Where(turno =>
                turno.TipoTurno == TipoTurno.Cumpleanios &&
                turno.EstadoTurno == EstadoTurno.Reservado &&
                turno.FechaHoraInicio >= fechaActual)
            .OrderBy(turno => turno.FechaHoraInicio)
            .Select(turno => new ReservaEspecialDto
            {
                TurnoId = turno.Id,
                NombreCliente = $"{turno.Cliente.Nombre} {turno.Cliente.Apellido}",
                TelefonoCliente = turno.Cliente.TelefonoCliente,
                FechaHoraInicio = turno.FechaHoraInicio,
                FechaHoraFin = turno.FechaHoraFin,
                PrecioTotal = turno.PrecioTotal,
                Observacion = turno.MotivoCancelacion,
                TextoEstado = "Reserva especial"
            })
            .ToListAsync();

        return reservasEspeciales;
    }

    // Calcula los horarios disponibles de una fecha puntual.
    // No guarda datos: solo consulta la configuracion, los horarios de atencion y los turnos ya reservados.
    public async Task<DisponibilidadTurnosDto> ObtenerDisponibilidadPorFechaAsync(DateOnly fecha)
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
        var excepcionHorario = await _contexto.ExcepcionesHorario
      .AsNoTracking()
      .FirstOrDefaultAsync(excepcion =>
          excepcion.CanchaId == cancha.Id &&
          excepcion.FechaDesde <= fecha &&
          excepcion.FechaHasta >= fecha);

        // Primero revisamos si el dueño cargó una excepcion para esta fecha puntual.
        // Ejemplo: feriado cerrado, vacaciones o apertura especial.
        if (excepcionHorario is not null)
        {
            if (!excepcionHorario.Abierto)
            {
                return new DisponibilidadTurnosDto
                {
                    Fecha = fecha,
                    Abierto = false,
                    MotivoNoDisponible = excepcionHorario.Motivo ?? "Cancha cerrada por excepción.",
                    Horarios = []
                };
            }

            if (excepcionHorario.HoraApertura is null || excepcionHorario.HoraCierre is null)
            {
                return new DisponibilidadTurnosDto
                {
                    Fecha = fecha,
                    Abierto = false,
                    MotivoNoDisponible = excepcionHorario.Motivo ?? "Cancha cerrada por excepción.",
                    Horarios = []
                };
            }
        }

        var horarioAtencion = await _contexto.HorariosAtencion
            .AsNoTracking()
            .FirstOrDefaultAsync(horario =>
                horario.CanchaId == cancha.Id &&
                horario.DiaSemana == fecha.DayOfWeek &&
                horario.Activo);

        if (horarioAtencion is null && excepcionHorario is null)
        {
            return new DisponibilidadTurnosDto
            {
                Fecha = fecha,
                Abierto = false,
                MotivoNoDisponible = "La cancha no tiene horario de atención para este día.",
                Horarios = []
            };
        }

        var horaApertura = excepcionHorario?.HoraApertura ?? horarioAtencion!.HoraApertura;
        var horaCierre = excepcionHorario?.HoraCierre ?? horarioAtencion!.HoraCierre;

        // Convertimos la fecha recibida a un rango completo del dia.
        // Esto nos permite buscar todos los turnos reservados entre las 00:00 y las 23:59:59.
        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = fecha.ToDateTime(TimeOnly.MaxValue);

        var fechaActual = DateTime.Now;

        // Traemos los turnos que bloquean disponibilidad.
        // Un turno reservado bloquea siempre.
        // Un turno pendiente tambien bloquea hasta que el dueno lo confirme o lo rechace.
        var turnosReservados = await _contexto.Turnos
            .AsNoTracking()
            .Where(turno =>
                turno.CanchaId == cancha.Id &&
                turno.FechaHoraInicio >= inicioDia &&
                turno.FechaHoraInicio <= finDia &&
                (
                    turno.EstadoTurno == EstadoTurno.Reservado ||
                    turno.EstadoTurno == EstadoTurno.EnEsperaDePago
                ))
            .ToListAsync();

        // Traemos los turnos fijos activos que aplican para la fecha consultada.
        // Ejemplo: si consultamos un martes, buscamos turnos fijos de martes.
        var turnosFijos = await _contexto.TurnosFijos
            .AsNoTracking()
            .Where(turnoFijo =>
                turnoFijo.CanchaId == cancha.Id &&
                turnoFijo.Activo &&
                turnoFijo.DiaSemana == fecha.DayOfWeek &&
                turnoFijo.FechaDesde <= fecha &&
                (
                    turnoFijo.FechaHasta == null ||
                    turnoFijo.FechaHasta >= fecha
                ))
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
        var fechaHoraInicio = fecha.ToDateTime(horaApertura);
        var fechaHoraCierre = fecha.ToDateTime(horaCierre);

        // Recorremos la franja de atencion en bloques del tamano de un turno.
        // Si la cancha abre 18:00 y cierra 23:00, generamos 18-19, 19-20, 20-21, etc.
        while (fechaHoraInicio.Add(duracionTurno) <= fechaHoraCierre)
        {
            // Calculamos el final del bloque actual.
            var fechaHoraFin = fechaHoraInicio.Add(duracionTurno);

            // Verificamos si el bloque actual se pisa con algun turno reservado.
            // Esta comparacion detecta superposiciones aunque el turno reservado no empiece exactamente a la misma hora.
            var turnoQueBloquea = turnosReservados.FirstOrDefault(turno =>
                fechaHoraInicio < turno.FechaHoraFin &&
                 fechaHoraFin > turno.FechaHoraInicio);

            // Revisamos si el bloque actual se pisa con algun turno fijo semanal.
            var turnoFijoQueBloquea = turnosFijos.FirstOrDefault(turnoFijo =>
            {
                var inicioTurnoFijo = fecha.ToDateTime(turnoFijo.HoraInicio);
                var finTurnoFijo = fecha.ToDateTime(turnoFijo.HoraFin);

                return fechaHoraInicio < finTurnoFijo &&
                    fechaHoraFin > inicioTurnoFijo;
            });

            var estaOcupado = turnoQueBloquea is not null || turnoFijoQueBloquea is not null;

            var estado = "Disponible";
            var textoEstado = "Disponible";

            if (turnoQueBloquea?.EstadoTurno == EstadoTurno.EnEsperaDePago)
            {
                estado = EstadoTurno.EnEsperaDePago.ToString();
                textoEstado = "Pendiente del dueño";
            }

            if (turnoQueBloquea?.EstadoTurno == EstadoTurno.Reservado)
            {
                estado = EstadoTurno.Reservado.ToString();
                textoEstado = turnoQueBloquea.TipoTurno == TipoTurno.Cumpleanios
                    ? "Cumpleaños"
                    : "Reservado";
            }

            if (turnoFijoQueBloquea is not null)
            {
                estado = "TurnoFijo";
                textoEstado = "Turno fijo";
            }

            horariosDisponibles.Add(new TurnoDisponibleDto
            {
                FechaHoraInicio = fechaHoraInicio,
                FechaHoraFin = fechaHoraFin,
                Disponible = !estaOcupado,
                Estado = estado,
                TextoEstado = textoEstado,
                MontoSena = configuracion.MontoSena,
                PrecioPorPersona = configuracion.PrecioPorPersona,
                PrecioTotal = precioTotal
            });

            // Avanzamos al siguiente bloque horario.
            fechaHoraInicio = fechaHoraInicio.Add(duracionTurno);
        }

        // Devolvemos todos los bloques generados para esa fecha, disponibles y ocupados.
        return new DisponibilidadTurnosDto
        {
            Fecha = fecha,
            Abierto = true,
            MotivoNoDisponible = null,
            Horarios = horariosDisponibles
        };
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

        var fechaActual = DateTime.Now;

        // Buscamos si ya existe un turno que bloquee el horario solicitado.
        // Un turno reservado bloquea siempre.
        // Un turno pendiente tambien bloquea hasta que el dueno lo confirme o lo rechace.
        var existeTurnoOcupado = await _contexto.Turnos
            .AnyAsync(turno =>
                turno.CanchaId == cancha.Id &&
                dto.FechaHoraInicio < turno.FechaHoraFin &&
                fechaHoraFin > turno.FechaHoraInicio &&
                (
                    turno.EstadoTurno == EstadoTurno.Reservado ||
                    turno.EstadoTurno == EstadoTurno.EnEsperaDePago
                ));

        if (existeTurnoOcupado)
        {
            throw new InvalidOperationException("El horario solicitado ya esta reservado.");
        }

        var turnosFijosDelDia = await _contexto.TurnosFijos.AsNoTracking().Where(turnoFijo =>
         turnoFijo.CanchaId == cancha.Id &&
         turnoFijo.Activo &&
         turnoFijo.DiaSemana == dto.FechaHoraInicio.DayOfWeek &&
         turnoFijo.FechaDesde <= fechaTurno &&
         (
             turnoFijo.FechaHasta == null ||
             turnoFijo.FechaHasta >= fechaTurno
         ))
     .ToListAsync();

        var existeTurnoFijoOcupado = turnosFijosDelDia.Any(turnoFijo =>
        {
            var inicioTurnoFijo = fechaTurno.ToDateTime(turnoFijo.HoraInicio);
            var finTurnoFijo = fechaTurno.ToDateTime(turnoFijo.HoraFin);

            return dto.FechaHoraInicio < finTurnoFijo &&
                fechaHoraFin > inicioTurnoFijo;
        });

        if (existeTurnoFijoOcupado)
        {
            throw new InvalidOperationException("El horario solicitado pertenece a un turno fijo.");
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
            EstadoTurno = EstadoTurno.EnEsperaDePago,
            PrecioPorPersonaAlReservar = configuracion.PrecioPorPersona,
            CantidadJugadores = configuracion.CantidadJugadoresPorTurno,
            PrecioTotal = precioTotal,
            MontoSena = configuracion.MontoSena,
            FechaVencimientoReserva = null
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
            MontoSena = turno.MontoSena,
            FechaVencimientoReserva = turno.FechaVencimientoReserva,
            EstadoTurno = turno.EstadoTurno.ToString(),
            TextoEstado = "Pendiente del dueño",
            AliasTransferencia = configuracion.AliasTransferencia,
            NombreTitularTransferencia = configuracion.NombreTitularTransferencia,
            MensajePagoReserva = configuracion.MensajePagoReserva,
            TokenCancelacion = turno.TokenCancelacion

        };
    }

    public async Task<TurnoCanceladoDto> RechazarTurnoPendienteAsync(int turnoId)
    {
        // Validamos que el id sea valido antes de consultar la base.
        if (turnoId <= 0)
        {
            throw new InvalidOperationException("El id del turno es obligatorio.");
        }

        // Buscamos el turno pendiente que el dueno quiere rechazar.
        var turno = await _contexto.Turnos
            .FirstOrDefaultAsync(turno => turno.Id == turnoId);

        if (turno is null)
        {
            throw new InvalidOperationException("No se encontro el turno indicado.");
        }

        // Solo permitimos rechazar reservas que todavia esperan aprobacion.
        // Un turno ya confirmado no deberia rechazarse desde esta accion.
        if (turno.EstadoTurno != EstadoTurno.EnEsperaDePago)
        {
            throw new InvalidOperationException("Solo se pueden rechazar turnos pendientes de aprobacion.");
        }

        var fechaActual = DateTime.Now;

        // Marcamos el turno como cancelado por el dueno.
        // No borramos el registro porque necesitamos conservar historial.
        turno.EstadoTurno = EstadoTurno.CanceladoPorDueno;
        turno.FechaCancelacion = fechaActual;
        turno.MotivoCancelacion = "Reserva pendiente rechazada por el dueno.";

        // Cancelamos notificaciones pendientes porque el turno ya no sigue activo.
        var notificacionesPendientes = await _contexto.NotificacionesWhatsApp
            .Where(notificacion =>
                notificacion.TurnoId == turno.Id &&
                notificacion.EstadoNotificacion == EstadoNotificacionWhatsApp.Pendiente)
            .ToListAsync();

        foreach (var notificacion in notificacionesPendientes)
        {
            notificacion.EstadoNotificacion = EstadoNotificacionWhatsApp.Cancelada;
        }

        await _contexto.SaveChangesAsync();

        return new TurnoCanceladoDto
        {
            TurnoId = turno.Id,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaCancelacion = fechaActual,
            EstadoTurno = turno.EstadoTurno.ToString(),
            Mensaje = "Reserva pendiente rechazada correctamente. El horario vuelve a estar disponible."
        };
    }


    public async Task<TurnoCanceladoDto> CancelarTurnoAsync(string tokenCancelacion)
    {
        // Validamos que el token venga informado.
        // Sin token no podemos identificar que turno quiere cancelar el cliente.
        if (string.IsNullOrWhiteSpace(tokenCancelacion))
        {
            throw new InvalidOperationException("El token de cancelacion es obligatorio.");
        }

        // Buscamos el turno por token.
        // Permitimos cancelar turnos confirmados y tambien reservas que todavia esperan aprobacion.
        var turno = await _contexto.Turnos.FirstOrDefaultAsync(turno =>turno.TokenCancelacion == tokenCancelacion &&
          (
              turno.EstadoTurno == EstadoTurno.Reservado ||
              turno.EstadoTurno == EstadoTurno.EnEsperaDePago
          ));

        if (turno is null)
        {
            throw new InvalidOperationException("No se encontro un turno reservado con el token indicado.");
        }

        // Buscamos la configuracion para saber cuantas horas antes se permite cancelar sin penalizacion.
        var configuracion = await _contexto.ConfiguracionesNegocio
            .FirstAsync();

        var fechaActual = DateTime.Now;
        var limiteCancelacion = turno.FechaHoraInicio.AddHours(-configuracion.HorasAnticipacionCancelacion);

        // Si el turno ya estaba confirmado, aplicamos la regla de cancelacion anticipada.
        // Si todavia estaba pendiente de aprobacion, dejamos cancelar para liberar el horario.
        if (turno.EstadoTurno == EstadoTurno.Reservado &&
      fechaActual > limiteCancelacion)
        {
            throw new InvalidOperationException("El turno ya no puede cancelarse porque esta dentro del limite de cancelacion.");
        }

        // Marcamos el turno como cancelado por el cliente.
        // No borramos el turno porque necesitamos conservar historial.
        turno.EstadoTurno = EstadoTurno.CanceladoPorCliente;
        turno.FechaCancelacion = fechaActual;
        turno.MotivoCancelacion = "Cancelado por el cliente.";

        // Cancelamos las notificaciones pendientes de WhatsApp asociadas al turno.
        // Por ejemplo, si habia un recordatorio programado, ya no deberia enviarse.
        var notificacionesPendientes = await _contexto.NotificacionesWhatsApp
            .Where(notificacion =>
                notificacion.TurnoId == turno.Id &&
                notificacion.EstadoNotificacion == EstadoNotificacionWhatsApp.Pendiente)
            .ToListAsync();

        foreach (var notificacion in notificacionesPendientes)
        {
            notificacion.EstadoNotificacion = EstadoNotificacionWhatsApp.Cancelada;
        }

        await _contexto.SaveChangesAsync();

        return new TurnoCanceladoDto
        {
            TurnoId = turno.Id,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaCancelacion = fechaActual,
            EstadoTurno = turno.EstadoTurno.ToString(),
            Mensaje = "Turno cancelado correctamente. El horario vuelve a estar disponible."
        };
    }

    public async Task<TurnoReservadoDto> CrearReservaEspecialAsync(CrearReservaEspecialDto dto)
    {
        // Validamos los datos principales porque esta reserva la carga el dueño manualmente.
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

        if (dto.FechaHoraInicio == default || dto.FechaHoraFin == default)
        {
            throw new InvalidOperationException("La fecha y hora de inicio y fin son obligatorias.");
        }

        if (dto.FechaHoraInicio <= DateTime.Now)
        {
            throw new InvalidOperationException("No se puede crear una reserva especial en una fecha pasada.");
        }

        if (dto.FechaHoraFin <= dto.FechaHoraInicio)
        {
            throw new InvalidOperationException("La hora de fin debe ser posterior a la hora de inicio.");
        }

        var configuracion = await _contexto.ConfiguracionesNegocio.FirstAsync();

        var cancha = await _contexto.Canchas.FirstAsync(cancha => cancha.Activa);

        var fechaTurno = DateOnly.FromDateTime(dto.FechaHoraInicio);
        var fechaFin = DateOnly.FromDateTime(dto.FechaHoraFin);

        if (fechaTurno != fechaFin)
        {
            throw new InvalidOperationException("La reserva especial debe iniciar y terminar en el mismo dia.");
        }

        // Revisamos que no se pise con reservas normales, cumpleaños u otros turnos pendientes.
        var existeTurnoOcupado = await _contexto.Turnos.AnyAsync(turno =>
            turno.CanchaId == cancha.Id &&
            dto.FechaHoraInicio < turno.FechaHoraFin &&
            dto.FechaHoraFin > turno.FechaHoraInicio &&
            (
                turno.EstadoTurno == EstadoTurno.Reservado ||
                turno.EstadoTurno == EstadoTurno.EnEsperaDePago
            ));

        if (existeTurnoOcupado)
        {
            throw new InvalidOperationException("La reserva especial se superpone con otro turno.");
        }

        var turnosFijosDelDia = await _contexto.TurnosFijos
            .AsNoTracking()
            .Where(turnoFijo =>
                turnoFijo.CanchaId == cancha.Id &&
                turnoFijo.Activo &&
                turnoFijo.DiaSemana == dto.FechaHoraInicio.DayOfWeek &&
                turnoFijo.FechaDesde <= fechaTurno &&
                (
                    turnoFijo.FechaHasta == null ||
                    turnoFijo.FechaHasta >= fechaTurno
                ))
            .ToListAsync();

        var existeTurnoFijoOcupado = turnosFijosDelDia.Any(turnoFijo =>
        {
            var inicioTurnoFijo = fechaTurno.ToDateTime(turnoFijo.HoraInicio);
            var finTurnoFijo = fechaTurno.ToDateTime(turnoFijo.HoraFin);

            return dto.FechaHoraInicio < finTurnoFijo &&
                dto.FechaHoraFin > inicioTurnoFijo;
        });

        if (existeTurnoFijoOcupado)
        {
            throw new InvalidOperationException("La reserva especial se superpone con un turno fijo.");
        }

        // Creamos el cliente asociado a la reserva especial.
        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            TelefonoCliente = dto.TelefonoCliente.Trim()
        };

        var duracionHoras = (decimal)(dto.FechaHoraFin - dto.FechaHoraInicio).TotalHours;
        var precioTotal = configuracion.PrecioPorPersona * configuracion.CantidadJugadoresPorTurno * duracionHoras;

        var turno = new Turno
        {
            CanchaId = cancha.Id,
            Cliente = cliente,
            FechaHoraInicio = dto.FechaHoraInicio,
            FechaHoraFin = dto.FechaHoraFin,
            TipoTurno = TipoTurno.Cumpleanios,
            EstadoTurno = EstadoTurno.Reservado,
            PrecioPorPersonaAlReservar = configuracion.PrecioPorPersona,
            CantidadJugadores = configuracion.CantidadJugadoresPorTurno,
            PrecioTotal = precioTotal,
            MontoSena = configuracion.MontoSena,
            FechaVencimientoReserva = null,
            FechaConfirmacion = DateTime.Now,
            MotivoCancelacion = dto.Observacion?.Trim()
        };

        _contexto.Turnos.Add(turno);

        await _contexto.SaveChangesAsync();

        return new TurnoReservadoDto
        {
            TurnoId = turno.Id,
            NombreCliente = $"{cliente.Nombre} {cliente.Apellido}",
            TelefonoCliente = cliente.TelefonoCliente,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaHoraFin = turno.FechaHoraFin,
            PrecioTotal = turno.PrecioTotal,
            MontoSena = turno.MontoSena,
            FechaVencimientoReserva = turno.FechaVencimientoReserva,
            EstadoTurno = turno.EstadoTurno.ToString(),
            TextoEstado = "Reserva especial",
            AliasTransferencia = configuracion.AliasTransferencia,
            NombreTitularTransferencia = configuracion.NombreTitularTransferencia,
            MensajePagoReserva = configuracion.MensajePagoReserva,
            TokenCancelacion = turno.TokenCancelacion
        };
    }

    public async Task<TurnoConfirmadoDto> ConfirmarTurnoAsync(int turnoId)
    {
        // Validamos que el id sea valido antes de buscar en la base.
        if (turnoId <= 0)
        {
            throw new InvalidOperationException("El id del turno es obligatorio.");
        }

        // Buscamos el turno que el dueno quiere confirmar.
        var turno = await _contexto.Turnos
            .FirstOrDefaultAsync(turno => turno.Id == turnoId);

        if (turno is null)
        {
            throw new InvalidOperationException("No se encontro el turno indicado.");
        }

        // Solo se pueden confirmar turnos que todavia esperan aprobacion del dueno.
        if (turno.EstadoTurno == EstadoTurno.Reservado)
        {
            throw new InvalidOperationException("El turno ya esta confirmado.");
        }

        if (turno.EstadoTurno != EstadoTurno.EnEsperaDePago)
        {
            throw new InvalidOperationException("Solo se pueden confirmar turnos pendientes de aprobacion.");
        }

        var fechaActual = DateTime.Now;

        // Confirmamos la reserva cuando el dueno decide aprobar el turno.
        turno.EstadoTurno = EstadoTurno.Reservado;
        turno.FechaConfirmacion = fechaActual;

        await _contexto.SaveChangesAsync();

        return new TurnoConfirmadoDto
        {
            TurnoId = turno.Id,
            FechaHoraInicio = turno.FechaHoraInicio,
            FechaHoraFin = turno.FechaHoraFin,
            FechaConfirmacion = fechaActual,
            PrecioTotal = turno.PrecioTotal,
            MontoSena = turno.MontoSena,
            EstadoTurno = turno.EstadoTurno.ToString(),
            Mensaje = "Turno confirmado correctamente."
        };
    }
}
