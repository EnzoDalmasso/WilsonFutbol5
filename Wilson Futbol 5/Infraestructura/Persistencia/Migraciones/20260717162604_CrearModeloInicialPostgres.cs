using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CrearModeloInicialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Canchas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canchas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TelefonoCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesNegocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrecioPorPersona = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadJugadoresPorTurno = table.Column<int>(type: "integer", nullable: false),
                    DuracionTurnoNormalMinutos = table.Column<int>(type: "integer", nullable: false),
                    DuracionCumpleaniosMinutos = table.Column<int>(type: "integer", nullable: false),
                    HorasAnticipacionCancelacion = table.Column<int>(type: "integer", nullable: false),
                    HorasAnticipacionRecordatorio = table.Column<int>(type: "integer", nullable: false),
                    ValorMultaInasistencia = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MinutosEsperaReserva = table.Column<int>(type: "integer", nullable: false),
                    MontoSena = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AliasTransferencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NombreTitularTransferencia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MensajePagoReserva = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesNegocio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CredencialesAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HashClave = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SaltClave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialesAdmin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SesionesAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HashToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesAdmin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExcepcionesHorario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanchaId = table.Column<int>(type: "integer", nullable: false),
                    FechaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaHasta = table.Column<DateOnly>(type: "date", nullable: false),
                    Abierto = table.Column<bool>(type: "boolean", nullable: false),
                    HoraApertura = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    HoraCierre = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcepcionesHorario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExcepcionesHorario_Canchas_CanchaId",
                        column: x => x.CanchaId,
                        principalTable: "Canchas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HorariosAtencion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanchaId = table.Column<int>(type: "integer", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraApertura = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraCierre = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosAtencion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosAtencion_Canchas_CanchaId",
                        column: x => x.CanchaId,
                        principalTable: "Canchas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanchaId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    FechaHoraInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaHoraFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoTurno = table.Column<int>(type: "integer", nullable: false),
                    EstadoTurno = table.Column<int>(type: "integer", nullable: false),
                    PrecioPorPersonaAlReservar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CantidadJugadores = table.Column<int>(type: "integer", nullable: false),
                    PrecioTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoSena = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaVencimientoReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaReserva = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TokenCancelacion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Turnos_Canchas_CanchaId",
                        column: x => x.CanchaId,
                        principalTable: "Canchas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Turnos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TurnosFijos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanchaId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaDesde = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaHasta = table.Column<DateOnly>(type: "date", nullable: true),
                    Observacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnosFijos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurnosFijos_Canchas_CanchaId",
                        column: x => x.CanchaId,
                        principalTable: "Canchas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnosFijos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificacionesWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    TelefonoDestino = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TipoNotificacion = table.Column<int>(type: "integer", nullable: false),
                    EstadoNotificacion = table.Column<int>(type: "integer", nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaEnviada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Intentos = table.Column<int>(type: "integer", nullable: false),
                    UltimoError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificacionesWhatsApp", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificacionesWhatsApp_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Penalizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    TurnoId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EstadoPenalizacion = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalizaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Penalizaciones_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Canchas",
                columns: new[] { "Id", "Activa", "Nombre" },
                values: new object[] { 1, true, "Wilson Futbol 5" });

            migrationBuilder.InsertData(
                table: "ConfiguracionesNegocio",
                columns: new[] { "Id", "AliasTransferencia", "CantidadJugadoresPorTurno", "DuracionCumpleaniosMinutos", "DuracionTurnoNormalMinutos", "FechaActualizacion", "HorasAnticipacionCancelacion", "HorasAnticipacionRecordatorio", "MensajePagoReserva", "MinutosEsperaReserva", "MontoSena", "NombreTitularTransferencia", "PrecioPorPersona", "ValorMultaInasistencia" },
                values: new object[] { 1, "wilson.futbol5", 10, 180, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 4, "Para confirmar la reserva, transferi la seña y envia el comprobante al dueño.", 30, 25000m, "Wilson Futbol 5", 5000m, 0m });

            migrationBuilder.InsertData(
                table: "HorariosAtencion",
                columns: new[] { "Id", "Activo", "CanchaId", "DiaSemana", "HoraApertura", "HoraCierre" },
                values: new object[,]
                {
                    { 1, true, 1, 1, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) },
                    { 2, true, 1, 2, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) },
                    { 3, true, 1, 3, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) },
                    { 4, true, 1, 4, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) },
                    { 5, true, 1, 5, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) },
                    { 6, true, 1, 6, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) },
                    { 7, true, 1, 0, new TimeOnly(18, 0, 0), new TimeOnly(23, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_TelefonoCliente",
                table: "Clientes",
                column: "TelefonoCliente");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorario_CanchaId_FechaDesde_FechaHasta",
                table: "ExcepcionesHorario",
                columns: new[] { "CanchaId", "FechaDesde", "FechaHasta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosAtencion_CanchaId_DiaSemana",
                table: "HorariosAtencion",
                columns: new[] { "CanchaId", "DiaSemana" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesWhatsApp_EstadoNotificacion_FechaProgramada",
                table: "NotificacionesWhatsApp",
                columns: new[] { "EstadoNotificacion", "FechaProgramada" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificacionesWhatsApp_TurnoId",
                table: "NotificacionesWhatsApp",
                column: "TurnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalizaciones_ClienteId",
                table: "Penalizaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalizaciones_TurnoId",
                table: "Penalizaciones",
                column: "TurnoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SesionesAdmin_Activa_FechaExpiracion",
                table: "SesionesAdmin",
                columns: new[] { "Activa", "FechaExpiracion" });

            migrationBuilder.CreateIndex(
                name: "IX_SesionesAdmin_HashToken",
                table: "SesionesAdmin",
                column: "HashToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_CanchaId_FechaHoraInicio_FechaHoraFin",
                table: "Turnos",
                columns: new[] { "CanchaId", "FechaHoraInicio", "FechaHoraFin" });

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_ClienteId",
                table: "Turnos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_TokenCancelacion",
                table: "Turnos",
                column: "TokenCancelacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TurnosFijos_CanchaId_DiaSemana_HoraInicio_HoraFin_Activo",
                table: "TurnosFijos",
                columns: new[] { "CanchaId", "DiaSemana", "HoraInicio", "HoraFin", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_TurnosFijos_ClienteId",
                table: "TurnosFijos",
                column: "ClienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesNegocio");

            migrationBuilder.DropTable(
                name: "CredencialesAdmin");

            migrationBuilder.DropTable(
                name: "ExcepcionesHorario");

            migrationBuilder.DropTable(
                name: "HorariosAtencion");

            migrationBuilder.DropTable(
                name: "NotificacionesWhatsApp");

            migrationBuilder.DropTable(
                name: "Penalizaciones");

            migrationBuilder.DropTable(
                name: "SesionesAdmin");

            migrationBuilder.DropTable(
                name: "TurnosFijos");

            migrationBuilder.DropTable(
                name: "Turnos");

            migrationBuilder.DropTable(
                name: "Canchas");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
