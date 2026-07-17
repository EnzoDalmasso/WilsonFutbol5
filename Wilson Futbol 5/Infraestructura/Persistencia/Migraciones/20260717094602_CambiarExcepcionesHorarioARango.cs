using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CambiarExcepcionesHorarioARango : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExcepcionesHorario_CanchaId_Fecha",
                table: "ExcepcionesHorario");

            migrationBuilder.RenameColumn(
                name: "Fecha",
                table: "ExcepcionesHorario",
                newName: "FechaHasta");

            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaDesde",
                table: "ExcepcionesHorario",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorario_CanchaId_FechaDesde_FechaHasta",
                table: "ExcepcionesHorario",
                columns: new[] { "CanchaId", "FechaDesde", "FechaHasta" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExcepcionesHorario_CanchaId_FechaDesde_FechaHasta",
                table: "ExcepcionesHorario");

            migrationBuilder.DropColumn(
                name: "FechaDesde",
                table: "ExcepcionesHorario");

            migrationBuilder.RenameColumn(
                name: "FechaHasta",
                table: "ExcepcionesHorario",
                newName: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_ExcepcionesHorario_CanchaId_Fecha",
                table: "ExcepcionesHorario",
                columns: new[] { "CanchaId", "Fecha" },
                unique: true);
        }
    }
}
