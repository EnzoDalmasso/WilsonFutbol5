using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Aplicacion.Servicios;
using Wilson_Futbol_5.Infraestructura.Persistencia;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
            {
                opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opciones.KnownProxies.Clear();
            });

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddScoped<IServicioTurnos, ServicioTurnos>();
            builder.Services.AddScoped<IServicioTurnosFijos, ServicioTurnosFijos>();
            builder.Services.AddScoped<IServicioExcepcionesHorario, ServicioExcepcionesHorario>();
            builder.Services.AddScoped<IServicioHorariosAtencion, ServicioHorariosAtencion>();
            builder.Services.AddScoped<IServicioConfiguracionNegocio, ServicioConfiguracionNegocio>();
            builder.Services.AddScoped<IServicioAutenticacionAdmin, ServicioAutenticacionAdmin>();

            // Al iniciar la app crea la primera contraseña del dueño si todavia no existe.
            builder.Services.AddHostedService<InicializadorCredencialAdmin>();

            // Leemos los origenes permitidos desde configuracion.
            // En local vienen de appsettings.Development.json y en produccion de variables de entorno.
            var origenesPermitidos = builder.Configuration
                .GetSection("Cors:OrigenesPermitidos")
                .Get<string[]>() ?? [];

            builder.Services.AddCors(opciones =>
            {
                opciones.AddPolicy("FrontendLocal", politica =>
                {
                    politica
                        .WithOrigins(origenesPermitidos)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Leemos la cadena de conexion que usara EF Core para conectarse a PostgreSQL/Supabase.
            var cadenaConexion = builder.Configuration.GetConnectionString("WilsonDb");

            if (string.IsNullOrWhiteSpace(cadenaConexion))
            {
                throw new InvalidOperationException("No se encontro la cadena de conexion 'WilsonDb'. Configurala en User Secrets o en las variables de entorno del hosting.");
            }

            // Registramos el DbContext con PostgreSQL para que EF Core pueda acceder a Supabase.
            builder.Services.AddDbContext<WilsonDbContext>(opciones =>
                opciones.UseNpgsql(cadenaConexion));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseForwardedHeaders();

            app.UseHttpsRedirection();

            app.UseCors("FrontendLocal");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
