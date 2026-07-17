using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Wilson_Futbol_5.Infraestructura.Seguridad;

// Filtro simple para proteger endpoints del panel del dueno.
// El frontend debe enviar la clave en el header X-Admin-Key.
public class RequiereClaveAdminAttribute : Attribute, IAuthorizationFilter
{
    private const string HeaderClaveAdmin = "X-Admin-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuracion = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var claveConfigurada = configuracion["SeguridadAdmin:Clave"];

        if (string.IsNullOrWhiteSpace(claveConfigurada))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                mensaje = "La clave de administrador no esta configurada."
            });

            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderClaveAdmin, out var claveRecibida))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                mensaje = "Falta la clave de administrador."
            });

            return;
        }

        if (claveRecibida != claveConfigurada)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                mensaje = "Clave de administrador invalida."
            });
        }
    }
}