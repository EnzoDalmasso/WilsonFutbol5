using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarEsperaPagoSena : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacion",
                table: "Turnos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimientoReserva",
                table: "Turnos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoSena",
                table: "Turnos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinutosEsperaReserva",
                table: "ConfiguracionesNegocio",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoSena",
                table: "ConfiguracionesNegocio",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "ConfiguracionesNegocio",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MinutosEsperaReserva", "MontoSena" },
                values: new object[] { 30, 25000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaConfirmacion",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "FechaVencimientoReserva",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "MontoSena",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "MinutosEsperaReserva",
                table: "ConfiguracionesNegocio");

            migrationBuilder.DropColumn(
                name: "MontoSena",
                table: "ConfiguracionesNegocio");
        }
    }
}
