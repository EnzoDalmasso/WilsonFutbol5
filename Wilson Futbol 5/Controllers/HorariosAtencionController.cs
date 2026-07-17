using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.HorariosAtencion;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5.Controllers;

[ApiController]
[RequiereClaveAdmin]
[Route("api/horarios-atencion")]
public class HorariosAtencionController : ControllerBase
{
    private readonly IServicioHorariosAtencion _servicioHorariosAtencion;

    // Recibimos el servicio por inyeccion de dependencias.
    // El controller maneja HTTP y el servicio maneja las reglas de horarios.
    public HorariosAtencionController(IServicioHorariosAtencion servicioHorariosAtencion)
    {
        _servicioHorariosAtencion = servicioHorariosAtencion;
    }

    // Endpoint para listar los horarios semanales configurados.
    // Ejemplo:
    // GET /api/horarios-atencion
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HorarioAtencionDto>>> ObtenerHorariosAtencion()
    {
        var horarios = await _servicioHorariosAtencion.ObtenerHorariosAtencionAsync();

        return Ok(horarios);
    }

    // Endpoint para modificar un horario semanal.
    // Ejemplo:
    // PUT /api/horarios-atencion/1
    [HttpPut("{horarioAtencionId:int}")]
    public async Task<ActionResult<HorarioAtencionDto>> ActualizarHorarioAtencion(
        int horarioAtencionId,
        [FromBody] ActualizarHorarioAtencionDto dto)
    {
        try
        {
            var horarioActualizado = await _servicioHorariosAtencion
                .ActualizarHorarioAtencionAsync(horarioAtencionId, dto);

            return Ok(horarioActualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }
}