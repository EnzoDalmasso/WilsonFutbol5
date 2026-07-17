namespace Wilson_Futbol_5.Aplicacion.DTOs.HorariosAtencion;

// Respuesta para mostrar la configuracion semanal de atencion en el panel del dueno.
public class HorarioAtencionDto
{
    public int HorarioAtencionId { get; set; }

    public DayOfWeek DiaSemana { get; set; }

    public string DiaSemanaTexto { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public TimeOnly HoraApertura { get; set; }

    public TimeOnly HoraCierre { get; set; }
}