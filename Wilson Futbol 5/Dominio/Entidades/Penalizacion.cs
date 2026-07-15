using Wilson_Futbol_5.Dominio.Enums;

namespace Wilson_Futbol_5.Dominio.Entidades;


// Registra una multa o penalizacion cuando un cliente no asiste y no cancela a tiempo.
// El monto queda definido por la configuracion vigente que maneje el dueno.

public class Penalizacion
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public int TurnoId { get; set; }

    public Turno Turno { get; set; } = null!;

    public decimal Monto { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public EstadoPenalizacion EstadoPenalizacion { get; set; } = EstadoPenalizacion.Pendiente;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public DateTime? FechaPago { get; set; }

    public string? Observacion { get; set; }
}
