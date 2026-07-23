using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;


// Define las operaciones principales relacionadas con turnos.
// La implementacion concreta va a estar en la capa de Aplicacion/Servicios.

public interface IServicioTurnos
{
    Task<DisponibilidadTurnosDto> ObtenerDisponibilidadPorFechaAsync(DateOnly fecha);

    // Devuelve los turnos que estan esperando que el dueno confirme la sena.
    Task<IReadOnlyList<TurnoPendienteConfirmacionDto>> ObtenerTurnosPendientesConfirmacionAsync();

    // Devuelve los turnos ya confirmados de una fecha para mostrar la agenda diaria del dueno.
    Task<IReadOnlyList<TurnoConfirmadoDelDiaDto>> ObtenerTurnosConfirmadosPorFechaAsync(DateOnly fecha);

    // Devuelve las reservas especiales ya cargadas por el dueno, como cumpleanos o eventos.
    Task<IReadOnlyList<ReservaEspecialDto>> ObtenerReservasEspecialesAsync();

    Task<TurnoReservadoDto> ReservarTurnoAsync(ReservarTurnoDto dto);

    // Crea una reserva especial cargada por el dueño, por ejemplo cumpleaños o eventos.
    Task<TurnoReservadoDto> CrearReservaEspecialAsync(CrearReservaEspecialDto dto);

    // Cancela una reserva especial para liberar el horario sin borrar el historial.
    Task<TurnoCanceladoDto> CancelarReservaEspecialAsync(int turnoId);

    Task<TurnoConfirmadoDto> ConfirmarTurnoAsync(int turnoId);

    // Permite que el dueno rechace una reserva pendiente si la sena no fue acreditada.
    Task<TurnoCanceladoDto> RechazarTurnoPendienteAsync(int turnoId);

    Task<TurnoCanceladoDto> CancelarTurnoAsync(string tokenCancelacion);
}
