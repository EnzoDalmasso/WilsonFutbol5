using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarExcepcionesHorario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcepcionesHorario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CanchaId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Abierto = table.Column<bool>(type: "bit", nullable: false),
                    HoraApertura = table.Column<TimeOnly>(type: "time", nullable: true),
                    HoraCierre = table.Column<TimeOnly>(type: "time", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorario_CanchaId_Fecha",
                table: "ExcepcionesHorario",
                columns: new[] { "CanchaId", "Fecha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcepcionesHorario");
        }
    }
}
