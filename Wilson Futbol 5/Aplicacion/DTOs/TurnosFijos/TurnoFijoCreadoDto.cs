namespace Wilson_Futbol_5.Aplicacion.DTOs.TurnosFijos;

// Respuesta que devolvemos cuando se crea un turno fijo.
public class TurnoFijoCreadoDto
{
    public int TurnoFijoId { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public DayOfWeek DiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public DateOnly FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    public string? Observacion { get; set; }

    public string Mensaje { get; set; } = string.Empty;
}
