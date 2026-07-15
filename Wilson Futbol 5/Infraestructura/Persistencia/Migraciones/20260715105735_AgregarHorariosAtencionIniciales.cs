using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarHorariosAtencionIniciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "HorariosAtencion",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
