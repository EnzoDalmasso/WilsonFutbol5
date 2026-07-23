using Wilson_Futbol_5.Dominio.Enums;

namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta para que el dueno vea la agenda confirmada de una fecha puntual.
public class TurnoConfirmadoDelDiaDto
{
    public int TurnoId { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string TelefonoCliente { get; set; } = string.Empty;

    public DateTime FechaHoraInicio { get; set; }

    public DateTime FechaHoraFin { get; set; }

    public TipoTurno TipoTurno { get; set; }

    public string TipoTurnoTexto { get; set; } = string.Empty;

    public decimal PrecioTotal { get; set; }
}
