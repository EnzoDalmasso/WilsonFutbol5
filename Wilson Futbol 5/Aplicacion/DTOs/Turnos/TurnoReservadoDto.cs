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

    public string TokenCancelacion { get; set; } = string.Empty;
}