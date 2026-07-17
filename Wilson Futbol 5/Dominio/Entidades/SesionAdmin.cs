namespace Wilson_Futbol_5.Dominio.Entidades;

// Representa un acceso temporal al panel del dueño.
// El frontend guarda el token real, pero la base guarda solo el hash del token.
public class SesionAdmin
{
    public int Id { get; set; }

    public string HashToken { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime FechaExpiracion { get; set; }

    public bool Activa { get; set; } = true;
}
