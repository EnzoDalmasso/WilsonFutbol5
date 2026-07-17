namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta para mostrar en el panel las reservas especiales cargadas por el dueno.
public class ReservaEspecialDto
{
    public int TurnoId { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public decimal PrecioTotal { get; set; }

    public string? Observacion { get; set; }

    public string TextoEstado { get; set; } = string.Empty;
}
