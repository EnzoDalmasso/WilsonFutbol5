using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wilson_Futbol_5.Dominio.Entidades;

namespace Wilson_Futbol_5.Infraestructura.Persistencia;


// Punto de entrada de EF Core hacia SQL Server. Aca se declaran las tablas y las reglas
// de mapeo para que el modelo de dominio se guarde de forma consistente.

public class WilsonDbContext : DbContext
{
    public WilsonDbContext(DbContextOptions<WilsonDbContext> opciones)
        : base(opciones)
    {
    }

    public DbSet<ExcepcionHorario> ExcepcionesHorario => Set<ExcepcionHorario>();

    public DbSet<Cancha> Canchas => Set<Cancha>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Turno> Turnos => Set<Turno>();

    public DbSet<TurnoFijo> TurnosFijos => Set<TurnoFijo>();

    public DbSet<HorarioAtencion> HorariosAtencion => Set<HorarioAtencion>();

    public DbSet<ConfiguracionNegocio> ConfiguracionesNegocio => Set<ConfiguracionNegocio>();

    public DbSet<Penalizacion> Penalizaciones => Set<Penalizacion>();

    public DbSet<NotificacionWhatsApp> NotificacionesWhatsApp => Set<NotificacionWhatsApp>();

    public DbSet<CredencialAdmin> CredencialesAdmin => Set<CredencialAdmin>();

    public DbSet<SesionAdmin> SesionesAdmin => Set<SesionAdmin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarCancha(modelBuilder.Entity<Cancha>());
        ConfigurarCliente(modelBuilder.Entity<Cliente>());
        ConfigurarTurno(modelBuilder.Entity<Turno>());
        ConfigurarTurnoFijo(modelBuilder.Entity<TurnoFijo>());
        ConfigurarHorarioAtencion(modelBuilder.Entity<HorarioAtencion>());
        ConfigurarConfiguracionNegocio(modelBuilder.Entity<ConfiguracionNegocio>());
        ConfigurarPenalizacion(modelBuilder.Entity<Penalizacion>());
        ConfigurarNotificacionWhatsApp(modelBuilder.Entity<NotificacionWhatsApp>());
        ConfigurarCredencialAdmin(modelBuilder.Entity<CredencialAdmin>());
        ConfigurarSesionAdmin(modelBuilder.Entity<SesionAdmin>());

        modelBuilder.Entity<ExcepcionHorario>(entidad =>
        {
            entidad.ToTable("ExcepcionesHorario");

            entidad.HasKey(excepcionHorario => excepcionHorario.Id);

            entidad.Property(excepcionHorario => excepcionHorario.Motivo)
                .HasMaxLength(200);

            entidad.HasOne(excepcionHorario => excepcionHorario.Cancha)
                .WithMany()
                .HasForeignKey(excepcionHorario => excepcionHorario.CanchaId)
                .OnDelete(DeleteBehavior.Restrict);

            entidad.HasIndex(excepcionHorario => new
            {
                excepcionHorario.CanchaId,
                excepcionHorario.FechaDesde,
                excepcionHorario.FechaHasta
            })
            .IsUnique();
        });
    }


    private static void ConfigurarCancha(EntityTypeBuilder<Cancha> entidad)
    {
        entidad.ToTable("Canchas");
        entidad.HasKey(cancha => cancha.Id);

        entidad.Property(cancha => cancha.Nombre).HasMaxLength(100).IsRequired();

        entidad.HasData(new Cancha
        {
            Id = 1,
            Nombre = "Wilson Futbol 5",
            Activa = true
        });
    }

    private static void ConfigurarCliente(EntityTypeBuilder<Cliente> entidad)
    {
        entidad.ToTable("Clientes");
        entidad.HasKey(cliente => cliente.Id);

        entidad.Property(cliente => cliente.Nombre)
            .HasMaxLength(80)
            .IsRequired();

        entidad.Property(cliente => cliente.Apellido)
            .HasMaxLength(80)
            .IsRequired();

        entidad.Property(cliente => cliente.TelefonoCliente)
            .HasMaxLength(20)
            .IsRequired();

        entidad.HasIndex(cliente => cliente.TelefonoCliente);
    }

    private static void ConfigurarTurno(EntityTypeBuilder<Turno> entidad)
    {
        entidad.ToTable("Turnos");
        entidad.HasKey(turno => turno.Id);

        entidad.Property(turno => turno.PrecioPorPersonaAlReservar)
            .HasPrecision(18, 2);

        entidad.Property(turno => turno.PrecioTotal)
            .HasPrecision(18, 2);

        entidad.Property(turno => turno.MontoSena)
            .HasPrecision(18, 2);

        entidad.Property(turno => turno.MotivoCancelacion)
            .HasMaxLength(300);

        entidad.Property(turno => turno.TokenCancelacion)
            .HasMaxLength(80)
            .IsRequired();

        entidad.HasIndex(turno => turno.TokenCancelacion)
            .IsUnique();

        entidad.HasIndex(turno => new { turno.CanchaId, turno.FechaHoraInicio, turno.FechaHoraFin });

        entidad.HasOne(turno => turno.Cancha)
            .WithMany(cancha => cancha.Turnos)
            .HasForeignKey(turno => turno.CanchaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidad.HasOne(turno => turno.Cliente)
            .WithMany(cliente => cliente.Turnos)
            .HasForeignKey(turno => turno.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarTurnoFijo(EntityTypeBuilder<TurnoFijo> entidad)
    {
        entidad.ToTable("TurnosFijos");
        entidad.HasKey(turnoFijo => turnoFijo.Id);

        entidad.Property(turnoFijo => turnoFijo.Observacion)
            .HasMaxLength(300);

        entidad.HasIndex(turnoFijo => new
        {
            turnoFijo.CanchaId,
            turnoFijo.DiaSemana,
            turnoFijo.HoraInicio,
            turnoFijo.HoraFin,
            turnoFijo.Activo
        });

        entidad.HasOne(turnoFijo => turnoFijo.Cancha)
            .WithMany(cancha => cancha.TurnosFijos)
            .HasForeignKey(turnoFijo => turnoFijo.CanchaId)
            .OnDelete(DeleteBehavior.Restrict);

        entidad.HasOne(turnoFijo => turnoFijo.Cliente)
            .WithMany(cliente => cliente.TurnosFijos)
            .HasForeignKey(turnoFijo => turnoFijo.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }


    private static void ConfigurarHorarioAtencion(EntityTypeBuilder<HorarioAtencion> entidad)
    {
        entidad.ToTable("HorariosAtencion");
        entidad.HasKey(horario => horario.Id);

        entidad.HasIndex(horario => new { horario.CanchaId, horario.DiaSemana });

        entidad.HasOne(horario => horario.Cancha)
            .WithMany(cancha => cancha.HorariosAtencion)
            .HasForeignKey(horario => horario.CanchaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cargamos horarios iniciales para poder probar la disponibilidad.
        // Por ahora usamos el mismo horario todos los dias: 18:00 a 23:00.
        // Mas adelante el dueno va a poder modificar estos horarios desde su panel.
        entidad.HasData(
            new HorarioAtencion
            {
                Id = 1,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Monday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            },
            new HorarioAtencion
            {
                Id = 2,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Tuesday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            },
            new HorarioAtencion
            {
                Id = 3,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Wednesday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            },
            new HorarioAtencion
            {
                Id = 4,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Thursday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            },
            new HorarioAtencion
            {
                Id = 5,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Friday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            },
            new HorarioAtencion
            {
                Id = 6,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Saturday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            },
            new HorarioAtencion
            {
                Id = 7,
                CanchaId = 1,
                DiaSemana = DayOfWeek.Sunday,
                HoraApertura = new TimeOnly(18, 0),
                HoraCierre = new TimeOnly(23, 0),
                Activo = true
            });
    }

    private static void ConfigurarConfiguracionNegocio(EntityTypeBuilder<ConfiguracionNegocio> entidad)
    {
        entidad.ToTable("ConfiguracionesNegocio");
        entidad.HasKey(configuracion => configuracion.Id);

        entidad.Property(configuracion => configuracion.PrecioPorPersona)
            .HasPrecision(18, 2);

        entidad.Property(configuracion => configuracion.ValorMultaInasistencia)
            .HasPrecision(18, 2);

        entidad.Property(configuracion => configuracion.MontoSena)
            .HasPrecision(18, 2);

        entidad.Property(configuracion => configuracion.AliasTransferencia)
            .HasMaxLength(100);

        entidad.Property(configuracion => configuracion.NombreTitularTransferencia)
            .HasMaxLength(120);

        entidad.Property(configuracion => configuracion.MensajePagoReserva)
            .HasMaxLength(300);

        entidad.HasData(new ConfiguracionNegocio
        {
            Id = 1,
            PrecioPorPersona = 5000,
            CantidadJugadoresPorTurno = 10,
            DuracionTurnoNormalMinutos = 60,
            DuracionCumpleaniosMinutos = 180,
            HorasAnticipacionCancelacion = 2,
            HorasAnticipacionRecordatorio = 4,
            ValorMultaInasistencia = 0,
            MinutosEsperaReserva = 30,
            MontoSena = 25000,
            AliasTransferencia = "wilson.futbol5",
            NombreTitularTransferencia = "Wilson Futbol 5",
            MensajePagoReserva = "Para confirmar la reserva, transferi la seña y envia el comprobante al dueño.",
            FechaActualizacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)

        });
    }

    private static void ConfigurarPenalizacion(EntityTypeBuilder<Penalizacion> entidad)
    {
        entidad.ToTable("Penalizaciones");
        entidad.HasKey(penalizacion => penalizacion.Id);

        entidad.Property(penalizacion => penalizacion.Monto)
            .HasPrecision(18, 2);

        entidad.Property(penalizacion => penalizacion.Motivo)
            .HasMaxLength(300)
            .IsRequired();

        entidad.Property(penalizacion => penalizacion.Observacion)
            .HasMaxLength(500);

        entidad.HasIndex(penalizacion => penalizacion.TurnoId)
            .IsUnique();

        entidad.HasOne(penalizacion => penalizacion.Cliente)
            .WithMany(cliente => cliente.Penalizaciones)
            .HasForeignKey(penalizacion => penalizacion.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entidad.HasOne(penalizacion => penalizacion.Turno)
            .WithOne(turno => turno.Penalizacion)
            .HasForeignKey<Penalizacion>(penalizacion => penalizacion.TurnoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurarNotificacionWhatsApp(EntityTypeBuilder<NotificacionWhatsApp> entidad)
    {
        entidad.ToTable("NotificacionesWhatsApp");
        entidad.HasKey(notificacion => notificacion.Id);

        entidad.Property(notificacion => notificacion.TelefonoDestino)
            .HasMaxLength(20)
            .IsRequired();

        entidad.Property(notificacion => notificacion.Mensaje)
            .HasMaxLength(1000)
            .IsRequired();

        entidad.Property(notificacion => notificacion.UltimoError)
            .HasMaxLength(1000);

        entidad.HasIndex(notificacion => new { notificacion.EstadoNotificacion, notificacion.FechaProgramada });

        entidad.HasOne(notificacion => notificacion.Turno)
            .WithMany(turno => turno.NotificacionesWhatsApp)
            .HasForeignKey(notificacion => notificacion.TurnoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarCredencialAdmin(EntityTypeBuilder<CredencialAdmin> entidad)
    {
        entidad.ToTable("CredencialesAdmin");
        entidad.HasKey(credencial => credencial.Id);

        entidad.Property(credencial => credencial.HashClave)
            .HasMaxLength(200)
            .IsRequired();

        entidad.Property(credencial => credencial.SaltClave)
            .HasMaxLength(100)
            .IsRequired();
    }

    private static void ConfigurarSesionAdmin(EntityTypeBuilder<SesionAdmin> entidad)
    {
        entidad.ToTable("SesionesAdmin");
        entidad.HasKey(sesion => sesion.Id);

        entidad.Property(sesion => sesion.HashToken)
            .HasMaxLength(200)
            .IsRequired();

        entidad.HasIndex(sesion => sesion.HashToken)
            .IsUnique();

        entidad.HasIndex(sesion => new { sesion.Activa, sesion.FechaExpiracion });
    }
}
