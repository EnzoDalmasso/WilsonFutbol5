using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wilson_Futbol_5.Aplicacion.Interfaces;

namespace Wilson_Futbol_5.Infraestructura.Seguridad;

// Filtro simple para proteger endpoints del panel del dueño.
// El frontend debe enviar el token temporal en el header X-Admin-Token.
public class RequiereClaveAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string HeaderTokenAdmin = "X-Admin-Token";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderTokenAdmin, out var tokenRecibido))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                mensaje = "Falta el token de administrador."
            });

            return;
        }

        var servicioAutenticacion = context.HttpContext.RequestServices
            .GetRequiredService<IServicioAutenticacionAdmin>();

        var tokenValido = await servicioAutenticacion.ValidarTokenAsync(tokenRecibido.ToString());

        if (!tokenValido)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                mensaje = "Sesion de administrador invalida o vencida."
            });
        }
    }
}
