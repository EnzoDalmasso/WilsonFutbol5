using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;


// Define las operaciones principales relacionadas con turnos.
// La implementacion concreta va a estar en la capa de Aplicacion/Servicios.

public interface IServicioTurnos
{
    Task<IReadOnlyList<TurnoDisponibleDto>> ObtenerDisponibilidadPorFechaAsync(DateOnly fecha);

    Task<TurnoReservadoDto> ReservarTurnoAsync(ReservarTurnoDto dto);
}