using Wilson_Futbol_5.Aplicacion.DTOs.AutenticacionAdmin;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;

// Define las operaciones de acceso al panel del dueño.
public interface IServicioAutenticacionAdmin
{
    Task InicializarCredencialAdminAsync();

    Task<LoginAdminRespuestaDto> IniciarSesionAsync(LoginAdminDto dto);

    Task CambiarClaveAsync(CambiarClaveAdminDto dto);

    Task ResetearClaveConSoporteAsync(ResetearClaveAdminSoporteDto dto);

    Task<bool> ValidarTokenAsync(string token);
}
