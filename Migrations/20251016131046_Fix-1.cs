using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Fix1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Votacion_ID",
                table: "SVE_Candidatos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Votacion_ID",
                table: "SVE_Candidatos");
        }
    }
}
