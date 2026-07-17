using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.ConfiguracionNegocio;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Contiene la logica para consultar y modificar precios, sena y datos de transferencia.
public class ServicioConfiguracionNegocio : IServicioConfiguracionNegocio
{
    private readonly WilsonDbContext _contexto;

    // Recibe el DbContext para leer y guardar la configuracion.
    public ServicioConfiguracionNegocio(WilsonDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ConfiguracionNegocioDto> ObtenerConfiguracionNegocioAsync()
    {
        // Actualmente usamos una sola configuracion activa del negocio.
        var configuracion = await _contexto.ConfiguracionesNegocio
            .AsNoTracking()
            .FirstAsync();

        return ConvertirADto(configuracion);
    }

    public async Task<ConfiguracionNegocioDto> ActualizarConfiguracionNegocioAsync(
        ActualizarConfiguracionNegocioDto dto)
    {
        if (dto.PrecioPorPersona < 0)
        {
            throw new InvalidOperationException("El precio por persona no puede ser negativo.");
        }

        if (dto.CantidadJugadoresPorTurno <= 0)
        {
            throw new InvalidOperationException("La cantidad de jugadores debe ser mayor a cero.");
        }

        if (dto.MontoSena < 0)
        {
            throw new InvalidOperationException("El monto de seña no puede ser negativo.");
        }

        var configuracion = await _contexto.ConfiguracionesNegocio
            .FirstAsync();

        configuracion.PrecioPorPersona = dto.PrecioPorPersona;
        configuracion.CantidadJugadoresPorTurno = dto.CantidadJugadoresPorTurno;
        configuracion.MontoSena = dto.MontoSena;
        configuracion.AliasTransferencia = dto.AliasTransferencia.Trim();
        configuracion.NombreTitularTransferencia = dto.NombreTitularTransferencia.Trim();
        configuracion.MensajePagoReserva = dto.MensajePagoReserva.Trim();
        configuracion.FechaActualizacion = DateTime.UtcNow;

        await _contexto.SaveChangesAsync();

        return ConvertirADto(configuracion);
    }

    private static ConfiguracionNegocioDto ConvertirADto(Dominio.Entidades.ConfiguracionNegocio configuracion)
    {
        var precioTotalTurno = configuracion.PrecioPorPersona * configuracion.CantidadJugadoresPorTurno;

        return new ConfiguracionNegocioDto
        {
            ConfiguracionNegocioId = configuracion.Id,
            PrecioPorPersona = configuracion.PrecioPorPersona,
            CantidadJugadoresPorTurno = configuracion.CantidadJugadoresPorTurno,
            PrecioTotalTurno = precioTotalTurno,
            MontoSena = configuracion.MontoSena,
            AliasTransferencia = configuracion.AliasTransferencia,
            NombreTitularTransferencia = configuracion.NombreTitularTransferencia,
            MensajePagoReserva = configuracion.MensajePagoReserva
        };
    }
}