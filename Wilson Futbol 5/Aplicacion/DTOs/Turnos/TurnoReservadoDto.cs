namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta que devolvemos cuando una reserva se crea correctamente.
public class TurnoReservadoDto
{
    public int TurnoId { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public decimal PrecioTotal { get; set; }

    public string AliasTransferencia { get; set; } = string.Empty;

    public string NombreTitularTransferencia { get; set; } = string.Empty;

    public string MensajePagoReserva { get; set; } = string.Empty;

    public decimal MontoSena { get; set; }

    public DateTime? FechaVencimientoReserva { get; set; }

    public string EstadoTurno { get; set; } = string.Empty;

    public string TextoEstado { get; set; } = string.Empty;

    public string TokenCancelacion { get; set; } = string.Empty;
}