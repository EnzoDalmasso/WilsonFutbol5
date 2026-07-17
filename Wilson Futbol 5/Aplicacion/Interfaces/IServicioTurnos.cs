using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;


// Define las operaciones principales relacionadas con turnos.
// La implementacion concreta va a estar en la capa de Aplicacion/Servicios.

public interface IServicioTurnos
{
    Task<DisponibilidadTurnosDto> ObtenerDisponibilidadPorFechaAsync(DateOnly fecha);

    // Devuelve los turnos que estan esperando que el dueno confirme la sena.
    Task<IReadOnlyList<TurnoPendienteConfirmacionDto>> ObtenerTurnosPendientesConfirmacionAsync();

    Task<TurnoReservadoDto> ReservarTurnoAsync(ReservarTurnoDto dto);

    Task<TurnoConfirmadoDto> ConfirmarTurnoAsync(int turnoId);

    // Permite que el dueno rechace una reserva pendiente si la sena no fue acreditada.
    Task<TurnoCanceladoDto> RechazarTurnoPendienteAsync(int turnoId);

    Task<TurnoCanceladoDto> CancelarTurnoAsync(string tokenCancelacion);
}