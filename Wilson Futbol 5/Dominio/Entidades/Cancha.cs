namespace Wilson_Futbol_5.Dominio.Entidades;

/// Representa una cancha disponible para reservas. Hoy el negocio tiene una sola cancha,
/// pero mantener esta entidad permite crecer sin rehacer el modelo.
public class Cancha
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public ICollection<HorarioAtencion> HorariosAtencion { get; set; } = new List<HorarioAtencion>();

    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}
