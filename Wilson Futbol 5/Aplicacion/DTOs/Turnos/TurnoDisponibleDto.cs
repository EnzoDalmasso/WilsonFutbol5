namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Representa un horario que el frontend puede mostrar en el calendario,
// indicando si esta disponible o bloqueado.
public class TurnoDisponibleDto
{
    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public bool Disponible { get; set; }

    public decimal PrecioPorPersona { get; set; }

    public decimal PrecioTotal { get; set; }
}
