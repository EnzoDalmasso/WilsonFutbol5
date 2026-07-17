namespace Wilson_Futbol_5.Aplicacion.DTOs.TurnosFijos;

// Respuesta para mostrar los turnos fijos cargados por el dueno.
public class TurnoFijoDto
{
    public int TurnoFijoId { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public DayOfWeek DiaSemana { get; set; }

    public string DiaSemanaTexto { get; set; } = string.Empty;

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public bool Activo { get; set; }

    public DateOnly FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    public string? Observacion { get; set; }
}