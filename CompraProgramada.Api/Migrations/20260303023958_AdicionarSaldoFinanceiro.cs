using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompraProgramada.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarSaldoFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaldoFinanceiro",
                table: "ContasGraficas",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoFinanceiro",
                table: "ContasGraficas");
        }
    }
}
