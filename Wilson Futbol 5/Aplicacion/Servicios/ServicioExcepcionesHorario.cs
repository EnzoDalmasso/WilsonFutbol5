using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.ExcepcionesHorario;
using Wilson_Futbol_5.Aplicacion.DTOs.Turnos;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Dominio.Entidades;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Contiene la logica para administrar excepciones de horario.
// Ejemplo: feriados cerrados, vacaciones o dias con horario especial.
public class ServicioExcepcionesHorario : IServicioExcepcionesHorario
{
    private readonly WilsonDbContext _contexto;

    // Recibe el DbContext para consultar cancha y guardar excepciones.
    public ServicioExcepcionesHorario(WilsonDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<ExcepcionHorarioDto>> ObtenerExcepcionesHorarioAsync()
    {
        // Mostramos primero las fechas mas cercanas.
        var excepciones = await _contexto.ExcepcionesHorario
            .AsNoTracking()
            .OrderBy(excepcion => excepcion.FechaDesde)
            .Select(excepcion => new ExcepcionHorarioDto
            {
                ExcepcionHorarioId = excepcion.Id,
                FechaDesde = excepcion.FechaDesde,
                FechaHasta = excepcion.FechaHasta,
                Abierto = excepcion.Abierto,
                HoraApertura = excepcion.HoraApertura,
                HoraCierre = excepcion.HoraCierre,
                Motivo = excepcion.Motivo,
                TextoEstado = excepcion.Abierto ? "Abierto especial" : "Cerrado"
            })
            .ToListAsync();

        return excepciones;
    }

    public async Task<ExcepcionHorarioDto> CrearExcepcionHorarioAsync(CrearExcepcionHorarioDto dto)
    {
        if (dto.FechaHasta < dto.FechaDesde)
        {
            throw new InvalidOperationException("La fecha hasta no puede ser menor que la fecha desde.");
        }

        // Si el dueno marca abierto, necesitamos horario de apertura y cierre.
        if (dto.Abierto && (dto.HoraApertura is null || dto.HoraCierre is null))
        {
            throw new InvalidOperationException("Si la cancha abre, debe indicar hora de apertura y cierre.");
        }

        if (dto.Abierto && dto.HoraApertura >= dto.HoraCierre)
        {
            throw new InvalidOperationException("La hora de apertura debe ser menor a la hora de cierre.");
        }

        // Por ahora usamos la cancha activa.
        // Si mas adelante hay varias canchas, el DTO deberia recibir CanchaId.
        var cancha = await _contexto.Canchas
            .FirstAsync(cancha => cancha.Activa);

        var existeCruce = await _contexto.ExcepcionesHorario
    .AnyAsync(excepcion =>
        excepcion.CanchaId == cancha.Id &&
        dto.FechaDesde <= excepcion.FechaHasta &&
        dto.FechaHasta >= excepcion.FechaDesde);

        if (existeCruce)
        {
            throw new InvalidOperationException("Ya existe una excepcion cargada que se cruza con ese rango de fechas.");
        }

        var excepcionHorario = new ExcepcionHorario
        {
            CanchaId = cancha.Id,
            FechaDesde = dto.FechaDesde,
            FechaHasta = dto.FechaHasta,
            Abierto = dto.Abierto,
            HoraApertura = dto.Abierto ? dto.HoraApertura : null,
            HoraCierre = dto.Abierto ? dto.HoraCierre : null,
            Motivo = dto.Motivo?.Trim()
        };

        _contexto.ExcepcionesHorario.Add(excepcionHorario);

        await _contexto.SaveChangesAsync();

        return new ExcepcionHorarioDto
        {
            ExcepcionHorarioId = excepcionHorario.Id,
            FechaDesde = excepcionHorario.FechaDesde,
            FechaHasta = excepcionHorario.FechaHasta,
            Abierto = excepcionHorario.Abierto,
            HoraApertura = excepcionHorario.HoraApertura,
            HoraCierre = excepcionHorario.HoraCierre,
            Motivo = excepcionHorario.Motivo,
            TextoEstado = excepcionHorario.Abierto ? "Abierto especial" : "Cerrado"
        };
    }

    public async Task EliminarExcepcionHorarioAsync(int excepcionHorarioId)
    {
        // Buscamos la excepcion cargada para poder eliminarla.
        var excepcionHorario = await _contexto.ExcepcionesHorario
            .FirstOrDefaultAsync(excepcion => excepcion.Id == excepcionHorarioId);

        if (excepcionHorario is null)
        {
            throw new InvalidOperationException("No se encontro la excepcion de horario.");
        }

        // Al eliminarla, esa fecha vuelve a tomar el horario semanal normal.
        _contexto.ExcepcionesHorario.Remove(excepcionHorario);

        await _contexto.SaveChangesAsync();
    }
}