using System.ComponentModel.DataAnnotations;

namespace Wilson_Futbol_5.Aplicacion.DTOs.ConfiguracionNegocio;

// Datos que recibe el backend cuando el dueno modifica precios y datos de pago.
public class ActualizarConfiguracionNegocioDto
{
    [Range(0, 999999999)]
    public decimal PrecioPorPersona { get; set; }

    [Range(1, 100)]
    public int CantidadJugadoresPorTurno { get; set; }

    [Range(0, 999999999)]
    public decimal MontoSena { get; set; }

    [MaxLength(100)]
    public string AliasTransferencia { get; set; } = string.Empty;

    [MaxLength(120)]
    public string NombreTitularTransferencia { get; set; } = string.Empty;

    [MaxLength(300)]
    public string MensajePagoReserva { get; set; } = string.Empty;
}