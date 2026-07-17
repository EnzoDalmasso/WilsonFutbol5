using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurnosController : ControllerBase
{
    private readonly IServicioTurnos _servicioTurnos;

    // Recibimos el servicio por inyeccion de dependencias.
    // El controller no sabe como se calcula la disponibilidad, solo delega esa tarea al servicio.
    public TurnosController(IServicioTurnos servicioTurnos)
    {
        _servicioTurnos = servicioTurnos;
    }

    // Endpoint para consultar la disponibilidad de turnos de una fecha.
    // Ejemplo de uso:
    // GET /api/turnos/disponibilidad?fecha=2026-07-15
    [HttpGet("disponibilidad")]
    public async Task<ActionResult<DisponibilidadTurnosDto>> ObtenerDisponibilidad(
        [FromQuery] DateOnly fecha)
    {
        // Validamos que la fecha venga correctamente desde la query string.
        // Si no viene, ASP.NET Core normalmente devuelve error 400 automaticamente.
        var turnosDisponibles = await _servicioTurnos.ObtenerDisponibilidadPorFechaAsync(fecha);

        // Devolvemos 200 OK con la lista de horarios disponibles y bloqueados.
        return Ok(turnosDisponibles);
    }

    // Endpoint para que el dueno vea las reservas que esperan aprobacion.
    // Ejemplo de uso:
    // GET /api/turnos/pendientes-confirmacion
    [HttpGet("pendientes-confirmacion")]
    [RequiereClaveAdmin]
    public async Task<ActionResult<IReadOnlyList<TurnoPendienteConfirmacionDto>>> ObtenerTurnosPendientesConfirmacion()
    {
        // El controller solo pide la lista al servicio.
        // La logica de filtrar turnos pendientes queda dentro de ServicioTurnos.
        var turnosPendientes = await _servicioTurnos.ObtenerTurnosPendientesConfirmacionAsync();

        return Ok(turnosPendientes);
    }

    // Endpoint para que el dueno vea los cumpleanos y reservas especiales cargadas.
    // Ejemplo de uso:
    // GET /api/turnos/reservas-especiales
    [HttpGet("reservas-especiales")]
    [RequiereClaveAdmin]
    public async Task<ActionResult<IReadOnlyList<ReservaEspecialDto>>> ObtenerReservasEspeciales()
    {
        // La consulta queda dentro del servicio para que el controller siga siendo liviano.
        var reservasEspeciales = await _servicioTurnos.ObtenerReservasEspecialesAsync();

        return Ok(reservasEspeciales);
    }

    // Endpoint para crear una reserva.
    // Ejemplo de uso:
    // POST /api/turnos/reservar
    [HttpPost("reservar")]
    public async Task<ActionResult<TurnoReservadoDto>> ReservarTurno([FromBody] ReservarTurnoDto dto)
    {
        try
        {
            // Delegamos la logica de negocio al servicio.
            // El controller solo recibe la peticion HTTP y devuelve una respuesta.
            var turnoReservado = await _servicioTurnos.ReservarTurnoAsync(dto);

            // Devolvemos 201 Created porque se creo un recurso nuevo: el turno.
            return CreatedAtAction(
                nameof(ObtenerDisponibilidad),
                new { fecha = DateOnly.FromDateTime(turnoReservado.FechaHoraInicio) },
                turnoReservado);
        }
        catch (InvalidOperationException ex)
        {
            // Si el servicio detecta una regla de negocio invalida,
            // devolvemos 400 Bad Request con el mensaje.
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Endpoint para que el dueño cargue una reserva especial, como cumpleaños o eventos.
    // Ejemplo de uso:
    // POST /api/turnos/reserva-especial
    [HttpPost("reserva-especial")]
    [RequiereClaveAdmin]
    public async Task<ActionResult<TurnoReservadoDto>> CrearReservaEspecial([FromBody] CrearReservaEspecialDto dto)
    {
        try
        {
            var turnoReservado = await _servicioTurnos.CrearReservaEspecialAsync(dto);

            return CreatedAtAction(
                nameof(ObtenerDisponibilidad),
                new { fecha = DateOnly.FromDateTime(turnoReservado.FechaHoraInicio) },
                turnoReservado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Endpoint para que el dueno confirme una reserva pendiente de aprobacion.
    // Ejemplo de uso:
    // POST /api/turnos/confirmar/1
    [HttpPost("confirmar/{turnoId:int}")]
    [RequiereClaveAdmin]
    public async Task<ActionResult<TurnoConfirmadoDto>> ConfirmarTurno(int turnoId)
    {
        try
        {
            // Delegamos la confirmacion al servicio.
            // El servicio valida si el turno existe y si todavia espera aprobacion.
            var turnoConfirmado = await _servicioTurnos.ConfirmarTurnoAsync(turnoId);

            return Ok(turnoConfirmado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Endpoint para que el dueno rechace una reserva pendiente de aprobacion.
    // Ejemplo de uso:
    // POST /api/turnos/rechazar-pendiente/1
    [HttpPost("rechazar-pendiente/{turnoId:int}")]
    [RequiereClaveAdmin]
    public async Task<ActionResult<TurnoCanceladoDto>> RechazarTurnoPendiente(int turnoId)
    {
        try
        {
            // Delegamos la regla al servicio.
            // El servicio valida que el turno exista y que todavia espere aprobacion.
            var turnoCancelado = await _servicioTurnos.RechazarTurnoPendienteAsync(turnoId);

            return Ok(turnoCancelado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Endpoint para cancelar una reserva usando el token de cancelacion.
    // Ejemplo de uso:
    // POST /api/turnos/cancelar/d70a96a58f5a4d1481d2622d687e9658
    [HttpPost("cancelar/{tokenCancelacion}")]
    public async Task<ActionResult<TurnoCanceladoDto>> CancelarTurno(string tokenCancelacion)
    {
        try
        {
            // Delegamos la regla de cancelacion al servicio.
            // El controller solo traduce la peticion HTTP a una respuesta.
            var turnoCancelado = await _servicioTurnos.CancelarTurnoAsync(tokenCancelacion);

            return Ok(turnoCancelado);
        }
        catch (InvalidOperationException ex)
        {
            // Si el token no existe, ya esta cancelado o esta fuera del limite permitido,
            // devolvemos 400 con un mensaje entendible.
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }
}
