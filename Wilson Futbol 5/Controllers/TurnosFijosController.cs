using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.TurnosFijos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5.Controllers;

[ApiController]
[RequiereClaveAdmin]
[Route("api/turnos-fijos")]
public class TurnosFijosController : ControllerBase
{
    private readonly IServicioTurnosFijos _servicioTurnosFijos;

    // Recibimos el servicio de turnos fijos por inyeccion de dependencias.
    // El controller solo maneja HTTP; la regla de negocio queda en el servicio.
    public TurnosFijosController(IServicioTurnosFijos servicioTurnosFijos)
    {
        _servicioTurnosFijos = servicioTurnosFijos;
    }

    // Endpoint para listar los turnos fijos cargados.
    // Ejemplo:
    // GET /api/turnos-fijos
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TurnoFijoDto>>> ObtenerTurnosFijos()
    {
        // Pedimos al servicio los turnos fijos con los datos del cliente.
        var turnosFijos = await _servicioTurnosFijos.ObtenerTurnosFijosAsync();

        return Ok(turnosFijos);
    }

    // Endpoint para crear un turno fijo semanal.
    // Ejemplo:
    // POST /api/turnos-fijos
    [HttpPost]
    public async Task<ActionResult<TurnoFijoCreadoDto>> CrearTurnoFijo([FromBody] CrearTurnoFijoDto dto)
    {
        try
        {
            // Delegamos la creacion al servicio.
            // El servicio valida horarios, cruces y cliente.
            var turnoFijoCreado = await _servicioTurnosFijos.CrearTurnoFijoAsync(dto);

            return Created(string.Empty, turnoFijoCreado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Endpoint para desactivar un turno fijo.
    // Ejemplo:
    // POST /api/turnos-fijos/5/desactivar
    [HttpPost("{turnoFijoId:int}/desactivar")]
    public async Task<ActionResult<TurnoFijoDto>> DesactivarTurnoFijo(int turnoFijoId)
    {
        try
        {
            // Desactivar no borra el registro; solo hace que deje de bloquear horarios.
            var turnoFijoDesactivado = await _servicioTurnosFijos.DesactivarTurnoFijoAsync(turnoFijoId);

            return Ok(turnoFijoDesactivado);
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
