namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta para mostrar una excepcion cargada en el panel del dueno.
public class ExcepcionHorarioDto
{
    public int ExcepcionHorarioId { get; set; }

    public DateOnly FechaDesde { get; set; }

    public DateOnly FechaHasta { get; set; }

    public bool Abierto { get; set; }

    public TimeOnly? HoraApertura { get; set; }

    public TimeOnly? HoraCierre { get; set; }

    public string? Motivo { get; set; }

    public string TextoEstado { get; set; } = string.Empty;
}
