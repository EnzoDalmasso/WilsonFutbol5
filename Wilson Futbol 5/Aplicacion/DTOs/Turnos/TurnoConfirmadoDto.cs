namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta que devolvemos cuando el dueno confirma una reserva en espera.
public class TurnoConfirmadoDto
{
    public int TurnoId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public DateTime FechaConfirmacion { get; set; }

    public decimal PrecioTotal { get; set; }

    public decimal MontoSena { get; set; }

    public string EstadoTurno { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;
}