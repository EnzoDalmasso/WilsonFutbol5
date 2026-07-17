namespace Wilson_Futbol_5.Aplicacion.DTOs.Turnos;

// Respuesta para que el dueno vea las reservas que esperan aprobacion.
public class TurnoPendienteConfirmacionDto
{
    // Id del turno que el dueno va a usar para confirmar o rechazar la reserva.
    public int TurnoId { get; set; }

    // Nombre completo de la persona que hizo la reserva.
    public string NombreCliente { get; set; } = string.Empty;

    // Telefono del cliente para contactarlo si necesita coordinar pago o confirmar datos.
    public string TelefonoCliente { get; set; } = string.Empty;

    // Fecha y hora en que empieza el turno reservado.
    public DateTime FechaHoraInicio { get; set; }

    // Fecha y hora en que termina el turno reservado.
    public DateTime FechaHoraFin { get; set; }

    // Precio total del turno.
    public decimal PrecioTotal { get; set; }

    // Monto sugerido que el dueno puede pedir como adelanto si corresponde.
    public decimal MontoSena { get; set; }

    // Dato heredado de la logica anterior; para reservas nuevas normalmente queda vacio.
    public DateTime? FechaVencimientoReserva { get; set; }

    // Estado interno del turno, por ejemplo EnEsperaDePago.
    public string EstadoTurno { get; set; } = string.Empty;

    // Texto mas amigable para mostrar en pantalla.
    public string TextoEstado { get; set; } = string.Empty;
}
