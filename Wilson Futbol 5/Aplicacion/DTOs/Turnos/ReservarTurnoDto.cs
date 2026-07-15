using Wilson_Futbol_5.Dominio.Enums;
using System.ComponentModel.DataAnnotations;

namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Datos que el frontend debe enviar cuando un cliente quiere reservar un turno.
public class ReservarTurnoDto
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

    [Required(ErrorMessage = "El tipo de turno es obligatorio.")]
    public TipoTurno TipoTurno { get; set; } = TipoTurno.Normal;
}