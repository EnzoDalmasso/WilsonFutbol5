using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.ConfiguracionNegocio;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5.Controllers;

[ApiController]
[RequiereClaveAdmin]
[Route("api/configuracion-negocio")]
public class ConfiguracionNegocioController : ControllerBase
{
    private readonly IServicioConfiguracionNegocio _servicioConfiguracionNegocio;

    // Recibimos el servicio por inyeccion de dependencias.
    // El controller maneja HTTP y el servicio maneja la regla de negocio.
    public ConfiguracionNegocioController(IServicioConfiguracionNegocio servicioConfiguracionNegocio)
    {
        _servicioConfiguracionNegocio = servicioConfiguracionNegocio;
    }

    // Endpoint para obtener precios, sena y datos de transferencia.
    // Ejemplo:
    // GET /api/configuracion-negocio
    [HttpGet]
    public async Task<ActionResult<ConfiguracionNegocioDto>> ObtenerConfiguracionNegocio()
    {
        var configuracion = await _servicioConfiguracionNegocio.ObtenerConfiguracionNegocioAsync();

        return Ok(configuracion);
    }

    // Endpoint para actualizar precios, sena y datos de transferencia.
    // Ejemplo:
    // PUT /api/configuracion-negocio
    [HttpPut]
    public async Task<ActionResult<ConfiguracionNegocioDto>> ActualizarConfiguracionNegocio(
        [FromBody] ActualizarConfiguracionNegocioDto dto)
    {
        try
        {
            var configuracion = await _servicioConfiguracionNegocio
                .ActualizarConfiguracionNegocioAsync(dto);

            return Ok(configuracion);
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