using Wilson_Futbol_5.Aplicacion.DTOs.ConfiguracionNegocio;

namespace Wilson_Futbol_5.Aplicacion.Interfaces;

// Define las operaciones para consultar y modificar la configuracion del negocio.
public interface IServicioConfiguracionNegocio
{
    // Devuelve precios, sena y datos de transferencia.
    Task<ConfiguracionNegocioDto> ObtenerConfiguracionNegocioAsync();

    // Actualiza precios, sena y datos de transferencia.
    Task<ConfiguracionNegocioDto> ActualizarConfiguracionNegocioAsync(
        ActualizarConfiguracionNegocioDto dto);
}