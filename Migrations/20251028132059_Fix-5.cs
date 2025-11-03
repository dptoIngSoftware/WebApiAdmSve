using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Fix5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                table: "SVE_Votos");

            migrationBuilder.DropForeignKey(
                name: "FK_SVE_Votos_SVE_Votantes_VotanteId",
                table: "SVE_Votos");

            migrationBuilder.DropIndex(
                name: "IX_SVE_Votos_CandidatoId",
                table: "SVE_Votos");

            migrationBuilder.DropIndex(
                name: "IX_SVE_Votos_VotanteId",
                table: "SVE_Votos");

            migrationBuilder.RenameColumn(
                name: "VotanteId",
                table: "SVE_Votos",
                newName: "Votante");

            migrationBuilder.RenameColumn(
                name: "CandidatoId",
                table: "SVE_Votos",
                newName: "Candidato");

            migrationBuilder.AddColumn<int>(
                name: "Votacion_ID",
                table: "SVE_Votos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Votacion_ID",
                table: "SVE_Votos");

            migrationBuilder.RenameColumn(
                name: "Votante",
                table: "SVE_Votos",
                newName: "VotanteId");

            migrationBuilder.RenameColumn(
                name: "Candidato",
                table: "SVE_Votos",
                newName: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votos_CandidatoId",
                table: "SVE_Votos",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votos_VotanteId",
                table: "SVE_Votos",
                column: "VotanteId");

            migrationBuilder.AddForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                table: "SVE_Votos",
                column: "CandidatoId",
                principalTable: "SVE_Candidatos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SVE_Votos_SVE_Votantes_VotanteId",
                table: "SVE_Votos",
                column: "VotanteId",
                principalTable: "SVE_Votantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
