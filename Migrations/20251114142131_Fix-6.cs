using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Fix6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaReenvio",
                table: "SVE_Votaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votos_Candidato",
                table: "SVE_Votos",
                column: "Candidato");

            migrationBuilder.AddForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_Candidato",
                table: "SVE_Votos",
                column: "Candidato",
                principalTable: "SVE_Candidatos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SVE_Votos_SVE_Candidatos_Candidato",
                table: "SVE_Votos");

            migrationBuilder.DropIndex(
                name: "IX_SVE_Votos_Candidato",
                table: "SVE_Votos");

            migrationBuilder.DropColumn(
                name: "FechaReenvio",
                table: "SVE_Votaciones");
        }
    }
}
