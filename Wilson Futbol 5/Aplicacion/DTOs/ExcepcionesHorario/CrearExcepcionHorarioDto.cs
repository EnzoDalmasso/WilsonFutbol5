using System.ComponentModel.DataAnnotations;

namespace Wilson_Futbol_5.Aplicacion.DTOs.ExcepcionesHorario;

// Datos que recibe el backend cuando el dueno carga una excepcion.
// Puede ser un feriado cerrado, vacaciones o un dia con horario especial.
public class CrearExcepcionHorarioDto
{
    [Required]
    public DateOnly FechaDesde { get; set; }

    [Required]
    public DateOnly FechaHasta { get; set; }

    public bool Abierto { get; set; }

    public TimeOnly? HoraApertura { get; set; }

    public TimeOnly? HoraCierre { get; set; }

    [MaxLength(200)]
    public string? Motivo { get; set; }
}