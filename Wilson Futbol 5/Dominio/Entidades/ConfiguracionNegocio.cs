namespace Wilson_Futbol_5.Dominio.Entidades;

// Centraliza los valores modificables por el dueno, como precios, duraciones y reglas
// de cancelacion. Esto evita dejar reglas de negocio escritas de forma fija en el codigo.

public class ConfiguracionNegocio
{
    public int Id { get; set; }

    public decimal PrecioPorPersona { get; set; } = 5000;

    public int CantidadJugadoresPorTurno { get; set; } = 10;

    public int DuracionTurnoNormalMinutos { get; set; } = 60;

    public int DuracionCumpleaniosMinutos { get; set; } = 180;

    public int HorasAnticipacionCancelacion { get; set; } = 2;

    public int HorasAnticipacionRecordatorio { get; set; } = 4;

    public decimal ValorMultaInasistencia { get; set; }

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
