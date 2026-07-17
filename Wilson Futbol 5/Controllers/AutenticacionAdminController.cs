using Microsoft.AspNetCore.Mvc;
using Wilson_Futbol_5.Aplicacion.DTOs.AutenticacionAdmin;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5.Controllers;

[ApiController]
[Route("api/autenticacion-admin")]
public class AutenticacionAdminController : ControllerBase
{
    private readonly IServicioAutenticacionAdmin _servicioAutenticacionAdmin;

    public AutenticacionAdminController(IServicioAutenticacionAdmin servicioAutenticacionAdmin)
    {
        _servicioAutenticacionAdmin = servicioAutenticacionAdmin;
    }

    // Login del dueño. Si la contraseña es correcta, devuelve un token temporal.
    // Ese token se usa despues para llamar a los endpoints protegidos.
    [HttpPost("login")]
    public async Task<ActionResult<LoginAdminRespuestaDto>> IniciarSesion([FromBody] LoginAdminDto dto)
    {
        try
        {
            var sesion = await _servicioAutenticacionAdmin.IniciarSesionAsync(dto);

            return Ok(sesion);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }

    // Cambio de contraseña del dueño.
    // Pedimos token valido y contraseña actual para evitar cambios accidentales.
    [HttpPost("cambiar-clave")]
    [RequiereClaveAdmin]
    public async Task<ActionResult> CambiarClave([FromBody] CambiarClaveAdminDto dto)
    {
        try
        {
            await _servicioAutenticacionAdmin.CambiarClaveAsync(dto);

            return Ok(new
            {
                mensaje = "Contraseña actualizada correctamente. Volve a iniciar sesion."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensaje = ex.Message
            });
        }
    }


    [HttpPost("resetear-clave-soporte")]
    public async Task<ActionResult> ResetearClaveConSoporte([FromBody] ResetearClaveAdminSoporteDto dto)
    {
        try
        {
            await _servicioAutenticacionAdmin.ResetearClaveConSoporteAsync(dto);

            return Ok(new
            {
                mensaje = "Contraseña reseteada correctamente."
            });
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
