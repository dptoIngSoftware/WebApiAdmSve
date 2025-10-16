using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Fix3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CandidatosXvoto",
                table: "SVE_Votaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CandidatosXvoto",
                table: "SVE_Votaciones");
        }
    }
}
