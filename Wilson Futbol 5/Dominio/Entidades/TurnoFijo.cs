namespace Wilson_Futbol_5.Dominio.Entidades;

// Representa un horario fijo semanal.
// Ejemplo: un cliente juega todos los martes de 18:00 a 19:00.
public class TurnoFijo
{
    public int Id { get; set; }

    public int CanchaId { get; set; }

    public Cancha Cancha { get; set; } = null!;

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    // Dia semanal en el que se repite el turno fijo.
    public DayOfWeek DiaSemana { get; set; }

    // Hora de inicio del turno fijo.
    public TimeOnly HoraInicio { get; set; }

    // Hora de fin del turno fijo.
    public TimeOnly HoraFin { get; set; }

    // Permite pausar el turno fijo sin borrarlo del historial.
    public bool Activo { get; set; } = true;

    // Fecha desde la que empieza a aplicar el turno fijo.
    public DateOnly FechaDesde { get; set; }

    // Fecha opcional hasta la que aplica el turno fijo.
    // Si queda null, el turno fijo sigue indefinidamente.
    public DateOnly? FechaHasta { get; set; }

    // Nota interna del dueno, por ejemplo: "Paga en efectivo".
    public string? Observacion { get; set; }

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
}