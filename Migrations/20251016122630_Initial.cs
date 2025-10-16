using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiVotacionElectronica.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SVE_Estados_Candidato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Estados_Candidato", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SVE_Estados_Votacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Estados_Votacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SVE_Sedes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Sedes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SVE_Votantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Votacion_ID = table.Column<int>(type: "int", nullable: false),
                    Rut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DV = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Nombre_Completo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ha_Votado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Votantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SVE_Candidatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rut = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DV = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Nombre_Completo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cargo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado_CandidatoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Candidatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SVE_Candidatos_SVE_Estados_Candidato_Estado_CandidatoId",
                        column: x => x.Estado_CandidatoId,
                        principalTable: "SVE_Estados_Candidato",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SVE_Votaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaTermino = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    SedeId = table.Column<int>(type: "int", nullable: false),
                    Estado_VotacionId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Votaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SVE_Votaciones_SVE_Estados_Votacion_Estado_VotacionId",
                        column: x => x.Estado_VotacionId,
                        principalTable: "SVE_Estados_Votacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SVE_Votaciones_SVE_Sedes_SedeId",
                        column: x => x.SedeId,
                        principalTable: "SVE_Sedes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SVE_Votos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VotanteId = table.Column<int>(type: "int", nullable: false),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    FechaVoto = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SVE_Votos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SVE_Votos_SVE_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "SVE_Candidatos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SVE_Votos_SVE_Votantes_VotanteId",
                        column: x => x.VotanteId,
                        principalTable: "SVE_Votantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Candidatos_Estado_CandidatoId",
                table: "SVE_Candidatos",
                column: "Estado_CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votaciones_Estado_VotacionId",
                table: "SVE_Votaciones",
                column: "Estado_VotacionId");

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votaciones_SedeId",
                table: "SVE_Votaciones",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votos_CandidatoId",
                table: "SVE_Votos",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_SVE_Votos_VotanteId",
                table: "SVE_Votos",
                column: "VotanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SVE_Votaciones");

            migrationBuilder.DropTable(
                name: "SVE_Votos");

            migrationBuilder.DropTable(
                name: "SVE_Estados_Votacion");

            migrationBuilder.DropTable(
                name: "SVE_Sedes");

            migrationBuilder.DropTable(
                name: "SVE_Candidatos");

            migrationBuilder.DropTable(
                name: "SVE_Votantes");

            migrationBuilder.DropTable(
                name: "SVE_Estados_Candidato");
        }
    }
}
