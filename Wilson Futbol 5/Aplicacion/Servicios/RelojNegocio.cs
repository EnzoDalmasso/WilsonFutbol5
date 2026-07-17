namespace Wilson_Futbol_5.Aplicacion.Servicios;

// Centraliza la hora operativa de la cancha.
// En produccion Render corre en Linux y puede usar UTC, por eso convertimos siempre a Argentina.
public static class RelojNegocio
{
    public static DateTime AhoraArgentina()
    {
        var zonaHorariaArgentina = ObtenerZonaHorariaArgentina();

        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaHorariaArgentina);
    }

    private static TimeZoneInfo ObtenerZonaHorariaArgentina()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        }
    }
}
