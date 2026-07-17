using Wilson_Futbol_5.Aplicacion.DTOs.HorariosAtencion;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;

// Define las operaciones para administrar los horarios semanales de la cancha.
public interface IServicioHorariosAtencion
{
    // Devuelve los horarios semanales para mostrarlos en el panel del dueno.
    Task<IReadOnlyList<HorarioAtencionDto>> ObtenerHorariosAtencionAsync();

    // Actualiza apertura, cierre y si ese dia esta activo.
    Task<HorarioAtencionDto> ActualizarHorarioAtencionAsync(int horarioAtencionId,ActualizarHorarioAtencionDto dto);
}