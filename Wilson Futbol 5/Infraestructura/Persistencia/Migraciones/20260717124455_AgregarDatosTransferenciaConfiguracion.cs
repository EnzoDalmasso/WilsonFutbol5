using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wilson_Futbol_5.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarDatosTransferenciaConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AliasTransferencia",
                table: "ConfiguracionesNegocio",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MensajePagoReserva",
                table: "ConfiguracionesNegocio",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NombreTitularTransferencia",
                table: "ConfiguracionesNegocio",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "ConfiguracionesNegocio",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AliasTransferencia", "MensajePagoReserva", "NombreTitularTransferencia" },
                values: new object[] { "wilson.futbol5", "Para confirmar la reserva, transferi la seña y envia el comprobante al dueño.", "Wilson Futbol 5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AliasTransferencia",
                table: "ConfiguracionesNegocio");

            migrationBuilder.DropColumn(
                name: "MensajePagoReserva",
                table: "ConfiguracionesNegocio");

            migrationBuilder.DropColumn(
                name: "NombreTitularTransferencia",
                table: "ConfiguracionesNegocio");
        }
    }
}
