namespace Wilson_Futbol_5.Dominio.Enums;


/// Permite controlar los intentos de envio y detectar mensajes pendientes o fallidos.

public enum EstadoNotificacionWhatsApp
{
    Pendiente = 1,
    Enviada = 2,
    Fallida = 3,
    Cancelada = 4
}
