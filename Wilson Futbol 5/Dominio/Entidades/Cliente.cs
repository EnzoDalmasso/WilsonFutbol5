namespace Wilson_Futbol_5.Dominio.Entidades;

// Guarda los datos minimos del cliente que reserva un turno, incluyendo el telefono
// normalizado para poder enviar confirmaciones y recordatorios por WhatsApp.
public class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();

    public ICollection<Penalizacion> Penalizaciones { get; set; } = new List<Penalizacion>();
}
