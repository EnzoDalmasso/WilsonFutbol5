namespace Wilson_Futbol_5.Aplicacion.DTOs.ConfiguracionNegocio;

// Respuesta para mostrar la configuracion modificable del negocio en el panel del dueno.
public class ConfiguracionNegocioDto
{
    public int ConfiguracionNegocioId { get; set; }

    public decimal PrecioPorPersona { get; set; }

    public int CantidadJugadoresPorTurno { get; set; }

    public decimal PrecioTotalTurno { get; set; }

    public decimal MontoSena { get; set; }

    public string AliasTransferencia { get; set; } = string.Empty;

    public string NombreTitularTransferencia { get; set; } = string.Empty;

    public string MensajePagoReserva { get; set; } = string.Empty;
}