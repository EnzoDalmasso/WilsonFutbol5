namespace Wilson_Futbol_5.Dominio.Entidades;

// Define las franjas horarias en las que la cancha acepta reservas para cada dia de la semana.
// El calendario del frontend se va a calcular a partir de estos horarios y los turnos ya reservados.
public class HorarioAtencion
{
    public int Id { get; set; }

    public int CanchaId { get; set; }

    public Cancha Cancha { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }

    public TimeOnly HoraApertura { get; set; }

    public TimeOnly HoraCierre { get; set; }

    public bool Activo { get; set; } = true;
}
