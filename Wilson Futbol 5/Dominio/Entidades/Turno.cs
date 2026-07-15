using Wilson_Futbol_5.Dominio.Enums;

namespace Wilson_Futbol_5.Dominio.Entidades;


/// Representa una reserva concreta de la cancha, con cliente, horario, precio congelado
/// al momento de reservar y estado operativo.

public class Turno
{
    public int Id { get; set; }

    public int CanchaId { get; set; }

    public Cancha Cancha { get; set; } = null!;

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public TipoTurno TipoTurno { get; set; } = TipoTurno.Normal;

    public EstadoTurno EstadoTurno { get; set; } = EstadoTurno.Reservado;

    public decimal PrecioPorPersonaAlReservar { get; set; }

    public int CantidadJugadores { get; set; }

    public decimal PrecioTotal { get; set; }

    public DateTime FechaReserva { get; set; } = DateTime.UtcNow;

    public DateTime? FechaCancelacion { get; set; }

    public string? MotivoCancelacion { get; set; }

    public string TokenCancelacion { get; set; } = Guid.NewGuid().ToString("N");

    public ICollection<NotificacionWhatsApp> NotificacionesWhatsApp { get; set; } = new List<NotificacionWhatsApp>();

    public Penalizacion? Penalizacion { get; set; }
}
