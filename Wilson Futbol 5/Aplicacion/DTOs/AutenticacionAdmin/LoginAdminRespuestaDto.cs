namespace Wilson_Futbol_5.Aplicacion.DTOs.AutenticacionAdmin;

public class LoginAdminRespuestaDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime FechaExpiracion { get; set; }
}
