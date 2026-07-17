
using Microsoft.EntityFrameworkCore;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Aplicacion.Servicios;
using Wilson_Futbol_5.Infraestructura.Persistencia;

namespace Wilson_Futbol_5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddScoped<IServicioTurnos, ServicioTurnos>();
            builder.Services.AddScoped<IServicioTurnosFijos, ServicioTurnosFijos>();
            builder.Services.AddScoped<IServicioExcepcionesHorario, ServicioExcepcionesHorario>();
            builder.Services.AddScoped<IServicioHorariosAtencion, ServicioHorariosAtencion>();
            builder.Services.AddScoped<IServicioConfiguracionNegocio, ServicioConfiguracionNegocio>();

            // Permitimos que el frontend local de Vite pueda llamar a la API durante el desarrollo.
            builder.Services.AddCors(opciones =>
            {
                opciones.AddPolicy("FrontendLocal", politica =>
                {
                    politica
                        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Registramos el DbContext con SQL Server para que EF Core pueda acceder a la base de datos.
            var cadenaConexion = builder.Configuration.GetConnectionString("WilsonDb")
                ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'WilsonDb'.");

            builder.Services.AddDbContext<WilsonDbContext>(opciones =>
                opciones.UseSqlServer(cadenaConexion));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("FrontendLocal");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
