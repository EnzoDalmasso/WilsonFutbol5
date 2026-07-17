using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarAutenticacionAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CredencialesAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HashClave = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SaltClave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredencialesAdmin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SesionesAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HashToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SesionesAdmin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SesionesAdmin_Activa_FechaExpiracion",
                table: "SesionesAdmin",
                columns: new[] { "Activa", "FechaExpiracion" });

            migrationBuilder.CreateIndex(
                name: "IX_SesionesAdmin_HashToken",
                table: "SesionesAdmin",
                column: "HashToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CredencialesAdmin");

            migrationBuilder.DropTable(
                name: "SesionesAdmin");
        }
    }
}
