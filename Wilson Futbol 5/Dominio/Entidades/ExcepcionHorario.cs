namespace Wilson_Futbol_5.Dominio.Entidades;

// Representa una excepcion para una fecha puntual.
// Ejemplo: feriado cerrado, vacaciones o dia especial con otro horario.
public class ExcepcionHorario
{
    public int Id { get; set; }

    public int CanchaId { get; set; }

    public Cancha Cancha { get; set; } = null!;

    // Fecha desde la que empieza a aplicar la excepcion.
    public DateOnly FechaDesde { get; set; }

    // Fecha hasta la que aplica la excepcion.
    // Si es un solo dia, FechaDesde y FechaHasta tienen el mismo valor.
    public DateOnly FechaHasta { get; set; }

    // Si esta en false, ese dia la cancha queda cerrada aunque el horario normal diga otra cosa.
    public bool Abierto { get; set; }

    // Se usan solo si Abierto es true.
    public TimeOnly? HoraApertura { get; set; }

    public TimeOnly? HoraCierre { get; set; }

    public string? Motivo { get; set; }
}