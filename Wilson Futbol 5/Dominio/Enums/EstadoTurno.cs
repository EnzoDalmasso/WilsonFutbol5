namespace Wilson_Futbol_5.Dominio.Enums;


/// Representa el estado actual de una reserva dentro del flujo operativo de la cancha.

public enum EstadoTurno
{
    Reservado = 1,
    CanceladoPorCliente = 2,
    CanceladoPorDueno = 3,
    Finalizado = 4,
    Inasistencia = 5
}
