using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.DTOs.HorariosAtencion;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Contiene la logica para administrar los horarios semanales de atencion.
public class ServicioHorariosAtencion : IServicioHorariosAtencion
{
    private readonly WilsonDbContext _contexto;

    // Recibe el DbContext para consultar y actualizar horarios.
    public ServicioHorariosAtencion(WilsonDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<HorarioAtencionDto>> ObtenerHorariosAtencionAsync()
    {
        // Listamos los horarios ordenados por dia de semana para mostrarlos en el panel del dueno.
        var horarios = await _contexto.HorariosAtencion
            .AsNoTracking()
            .OrderBy(horario => horario.DiaSemana)
            .Select(horario => new HorarioAtencionDto
            {
                HorarioAtencionId = horario.Id,
                DiaSemana = horario.DiaSemana,
                DiaSemanaTexto = ObtenerNombreDia(horario.DiaSemana),
                Activo = horario.Activo,
                HoraApertura = horario.HoraApertura,
                HoraCierre = horario.HoraCierre
            })
            .ToListAsync();

        return horarios;
    }

    public async Task<HorarioAtencionDto> ActualizarHorarioAtencionAsync(
        int horarioAtencionId,
        ActualizarHorarioAtencionDto dto)
    {
        if (dto.Activo && dto.HoraApertura >= dto.HoraCierre)
        {
            throw new InvalidOperationException("La hora de apertura debe ser menor a la hora de cierre.");
        }

        var horario = await _contexto.HorariosAtencion
            .FirstOrDefaultAsync(horario => horario.Id == horarioAtencionId);

        if (horario is null)
        {
            throw new InvalidOperationException("No se encontro el horario de atencion.");
        }

        horario.Activo = dto.Activo;
        horario.HoraApertura = dto.HoraApertura;
        horario.HoraCierre = dto.HoraCierre;

        await _contexto.SaveChangesAsync();

        return new HorarioAtencionDto
        {
            HorarioAtencionId = horario.Id,
            DiaSemana = horario.DiaSemana,
            DiaSemanaTexto = ObtenerNombreDia(horario.DiaSemana),
            Activo = horario.Activo,
            HoraApertura = horario.HoraApertura,
            HoraCierre = horario.HoraCierre
        };
    }

    private static string ObtenerNombreDia(DayOfWeek diaSemana)
    {
        return diaSemana switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miercoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sabado",
            DayOfWeek.Sunday => "Domingo",
            _ => diaSemana.ToString()
        };
    }
}