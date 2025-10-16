using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Fix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                table: "SVE_Votos");

            migrationBuilder.AlterColumn<int>(
                name: "CandidatoId",
                table: "SVE_Votos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                table: "SVE_Votos",
                column: "CandidatoId",
                principalTable: "SVE_Candidatos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                table: "SVE_Votos");

            migrationBuilder.AlterColumn<int>(
                name: "CandidatoId",
                table: "SVE_Votos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                table: "SVE_Votos",
                column: "CandidatoId",
                principalTable: "SVE_Candidatos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
