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

    // Cantidad de minutos que el turno queda bloqueado mientras el cliente transfiere la seña.
    public int MinutosEsperaReserva { get; set; } = 1;

    // Monto que el dueño solicita como seña para confirmar la reserva.
    public decimal MontoSena { get; set; } = 25000;

    // Alias o CVU donde el cliente debe transferir la sena.
    public string AliasTransferencia { get; set; } = string.Empty;

    // Nombre del titular de la cuenta que recibe la transferencia.
    public string NombreTitularTransferencia { get; set; } = string.Empty;

    // Mensaje breve que se muestra al cliente despues de enviar la reserva.
    public string MensajePagoReserva { get; set; } = string.Empty;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
