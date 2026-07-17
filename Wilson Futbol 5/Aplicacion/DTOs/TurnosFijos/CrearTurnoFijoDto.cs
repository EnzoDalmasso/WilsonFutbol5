using System.ComponentModel.DataAnnotations;

namespace Wilson_Futbol_5.Aplicacion.DTOs.TurnosFijos;

// Datos que recibe el backend cuando el dueno crea un turno fijo semanal.
public class CrearTurnoFijoDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(80)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio.")]
    [MaxLength(20)]
    public string TelefonoCliente { get; set; } = string.Empty;

    [Required]
    public DayOfWeek DiaSemana { get; set; }

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }

    [Required]
    public DateOnly FechaDesde { get; set; }

    public DateOnly? FechaHasta { get; set; }

    [MaxLength(300)]
    public string? Observacion { get; set; }
}