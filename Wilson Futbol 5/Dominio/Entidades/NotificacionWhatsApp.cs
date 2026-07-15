using Wilson_Futbol_5.Dominio.Enums;

namespace Wilson_Futbol_5.Dominio.Entidades;

/// Guarda los mensajes que el sistema debe enviar por WhatsApp. La reserva solo crea
/// la notificacion; otro servicio se encarga del envio para mantener bajo acoplamiento.

public class NotificacionWhatsApp
{
    public int Id { get; set; }

    public int TurnoId { get; set; }

    public Turno Turno { get; set; } = null!;

    public string TelefonoDestino { get; set; } = string.Empty;

    public TipoNotificacionWhatsApp TipoNotificacion { get; set; }

    public EstadoNotificacionWhatsApp EstadoNotificacion { get; set; } = EstadoNotificacionWhatsApp.Pendiente;

    public string Mensaje { get; set; } = string.Empty;

    public DateTime FechaProgramada { get; set; }

    public DateTime? FechaEnviada { get; set; }

    public int Intentos { get; set; }

    public string? UltimoError { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
