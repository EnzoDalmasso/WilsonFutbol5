using System.ComponentModel.DataAnnotations;

namespace Wilson_Futbol_5.Aplicacion.DTOs.HorariosAtencion;

// Datos que recibe el backend cuando el dueno modifica un horario semanal.
public class ActualizarHorarioAtencionDto
{
    public bool Activo { get; set; }

    [Required]
    public TimeOnly HoraApertura { get; set; }

    [Required]
    public TimeOnly HoraCierre { get; set; }
}