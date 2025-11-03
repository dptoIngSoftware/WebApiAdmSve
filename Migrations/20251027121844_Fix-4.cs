using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Fix4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Correo_Enviado",
                table: "SVE_Votantes",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Correo_Enviado",
                table: "SVE_Votantes");
        }
    }
}
