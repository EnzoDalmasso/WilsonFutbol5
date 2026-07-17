using Wilson_Futbol_5.Aplicacion.Interfaces;

namespace Wilson_Futbol_5.Infraestructura.Seguridad;

// Crea la primera contraseña del dueño cuando la base todavia no tiene ninguna.
// Despues el dueño puede cambiarla desde el panel.
public class InicializadorCredencialAdmin : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public InicializadorCredencialAdmin(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var servicioAutenticacion = scope.ServiceProvider.GetRequiredService<IServicioAutenticacionAdmin>();

        await servicioAutenticacion.InicializarCredencialAdminAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
