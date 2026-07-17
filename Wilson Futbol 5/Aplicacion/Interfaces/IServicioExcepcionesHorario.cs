using Wilson_Futbol_5.Aplicacion.DTOs.ExcepcionesHorario;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;

// Define las operaciones para administrar feriados, vacaciones y horarios especiales.
public interface IServicioExcepcionesHorario
{
    // Devuelve las excepciones cargadas para que el dueno las vea en el panel.
    Task<IReadOnlyList<ExcepcionHorarioDto>> ObtenerExcepcionesHorarioAsync();

    // Crea una excepcion de horario para una fecha puntual.
    Task<ExcepcionHorarioDto> CrearExcepcionHorarioAsync(CrearExcepcionHorarioDto dto);

    // Elimina una excepcion para que esa fecha vuelva a usar el horario normal.
    Task EliminarExcepcionHorarioAsync(int excepcionHorarioId);
}