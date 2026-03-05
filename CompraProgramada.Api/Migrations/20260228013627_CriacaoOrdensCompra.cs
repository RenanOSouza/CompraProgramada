using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompraProgramada.Api.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoOrdensCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensCesta_CestasRecomendacao_CestaId",
                table: "ItensCesta");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CestasRecomendacao",
                table: "CestasRecomendacao");

            migrationBuilder.RenameTable(
                name: "CestasRecomendacao",
                newName: "TopFive");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "TopFive",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopFive",
                table: "TopFive",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OrdensCompra",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DataOperacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Ticker = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuantidadeTotal = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ValorTotalOperacao = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TipoMercado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensCompra", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Distribuicoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdemCompraId = table.Column<long>(type: "bigint", nullable: false),
                    ContaGraficaId = table.Column<long>(type: "bigint", nullable: false),
                    Ticker = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    ValorRateado = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distribuicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Distribuicoes_ContasGraficas_ContaGraficaId",
                        column: x => x.ContaGraficaId,
                        principalTable: "ContasGraficas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Distribuicoes_OrdensCompra_OrdemCompraId",
                        column: x => x.OrdemCompraId,
                        principalTable: "OrdensCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Distribuicoes_ContaGraficaId",
                table: "Distribuicoes",
                column: "ContaGraficaId");

            migrationBuilder.CreateIndex(
                name: "IX_Distribuicoes_OrdemCompraId",
                table: "Distribuicoes",
                column: "OrdemCompraId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensCesta_TopFive_CestaId",
                table: "ItensCesta",
                column: "CestaId",
                principalTable: "TopFive",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItensCesta_TopFive_CestaId",
                table: "ItensCesta");

            migrationBuilder.DropTable(
                name: "Distribuicoes");

            migrationBuilder.DropTable(
                name: "OrdensCompra");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopFive",
                table: "TopFive");

            migrationBuilder.RenameTable(
                name: "TopFive",
                newName: "CestasRecomendacao");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CestasRecomendacao",
                newName: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CestasRecomendacao",
                table: "CestasRecomendacao",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItensCesta_CestasRecomendacao_CestaId",
                table: "ItensCesta",
                column: "CestaId",
                principalTable: "CestasRecomendacao",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
