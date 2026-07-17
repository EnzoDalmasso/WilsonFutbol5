namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta completa de disponibilidad para que el front sepa si la cancha abre,
// por que no hay horarios y que lista de turnos debe mostrar.
public class DisponibilidadTurnosDto
{
    public DateOnly Fecha { get; set; }

    public bool Abierto { get; set; }

    public string? MotivoNoDisponible { get; set; }

    public IReadOnlyList<TurnoDisponibleDto> Horarios { get; set; } = [];
}