using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.ExcepcionesHorario;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5.Controllers;

[ApiController]
[RequiereClaveAdmin]
[Route("api/excepciones-horario")]
public class ExcepcionesHorarioController : ControllerBase
{
    private readonly IServicioExcepcionesHorario _servicioExcepcionesHorario;

    // Recibimos el servicio por inyeccion de dependencias.
    // El controller maneja HTTP y el servicio maneja la regla de negocio.
    public ExcepcionesHorarioController(IServicioExcepcionesHorario servicioExcepcionesHorario)
    {
        _servicioExcepcionesHorario = servicioExcepcionesHorario;
    }

    // Endpoint para listar feriados, vacaciones y dias especiales cargados.
    // Ejemplo:
    // GET /api/excepciones-horario
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExcepcionHorarioDto>>> ObtenerExcepcionesHorario()
    {
        var excepciones = await _servicioExcepcionesHorario.ObtenerExcepcionesHorarioAsync();

        return Ok(excepciones);
    }

    // Endpoint para crear una excepcion de horario.
    // Ejemplo:
    // POST /api/excepciones-horario
    [HttpPost]
    public async Task<ActionResult<ExcepcionHorarioDto>> CrearExcepcionHorario([FromBody] CrearExcepcionHorarioDto dto)
    {
        try
        {
            var excepcionCreada = await _servicioExcepcionesHorario.CrearExcepcionHorarioAsync(dto);

            return Created(string.Empty, excepcionCreada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Endpoint para eliminar una excepcion de horario.
    // Ejemplo:
    // DELETE /api/excepciones-horario/3
    [HttpDelete("{excepcionHorarioId:int}")]
    public async Task<IActionResult> EliminarExcepcionHorario(int excepcionHorarioId)
    {
        try
        {
            await _servicioExcepcionesHorario.EliminarExcepcionHorarioAsync(excepcionHorarioId);

            return NoContent();
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
