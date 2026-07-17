namespace Wilson_Futbol_5.Dominio.Entidades;

// Guarda la contraseña del panel de dueño de forma segura.
// No guardamos la contraseña real, solo su hash y salt.
public class CredencialAdmin
{
    public int Id { get; set; }

    public string HashClave { get; set; } = string.Empty;

    public string SaltClave { get; set; } = string.Empty;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
