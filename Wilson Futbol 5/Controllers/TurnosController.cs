using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;

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
    public async Task<ActionResult<IReadOnlyList<TurnoDisponibleDto>>> ObtenerDisponibilidad(
        [FromQuery] DateOnly fecha)
    {
        // Validamos que la fecha venga correctamente desde la query string.
        // Si no viene, ASP.NET Core normalmente devuelve error 400 automaticamente.
        var turnosDisponibles = await _servicioTurnos.ObtenerDisponibilidadPorFechaAsync(fecha);

        // Devolvemos 200 OK con la lista de horarios disponibles y bloqueados.
        return Ok(turnosDisponibles);
    }
}