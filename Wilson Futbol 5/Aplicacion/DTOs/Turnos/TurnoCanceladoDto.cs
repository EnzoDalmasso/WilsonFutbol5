namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta que devolvemos cuando una reserva se cancela correctamente.
public class TurnoCanceladoDto
{
    public int TurnoId { get; set; }

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaCancelacion { get; set; }

    public string EstadoTurno { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;
}