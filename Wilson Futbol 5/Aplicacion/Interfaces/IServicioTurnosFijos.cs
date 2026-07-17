using Wilson_Futbol_5.Aplicacion.DTOs.TurnosFijos;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;

// Define las operaciones para administrar turnos fijos semanales.
public interface IServicioTurnosFijos
{
    // Devuelve los turnos fijos cargados para que el dueno los pueda revisar.
    Task<IReadOnlyList<TurnoFijoDto>> ObtenerTurnosFijosAsync();

    // Crea un turno fijo semanal para un cliente.
    Task<TurnoFijoCreadoDto> CrearTurnoFijoAsync(CrearTurnoFijoDto dto);

    // Desactiva un turno fijo para que deje de bloquear horarios futuros.
    Task<TurnoFijoDto> DesactivarTurnoFijoAsync(int turnoFijoId);
}
