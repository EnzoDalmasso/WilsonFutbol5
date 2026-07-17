using System.ComponentModel.DataAnnotations;

namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Datos que carga el dueño para bloquear una reserva especial, como cumpleaños o eventos.
public class CrearReservaEspecialDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El telefono es obligatorio.")]
    [StringLength(20, ErrorMessage = "El telefono no puede superar los 20 caracteres.")]
    public string TelefonoCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria.")]
    public DateTime FechaHoraInicio { get; set; }

    [Required(ErrorMessage = "La fecha y hora de fin es obligatoria.")]
    public DateTime FechaHoraFin { get; set; }

    [StringLength(250, ErrorMessage = "La observacion no puede superar los 250 caracteres.")]
    public string? Observacion { get; set; }
}