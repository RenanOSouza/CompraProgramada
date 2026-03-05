using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompraProgramada.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDataDesativacaoCesta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataDesativacao",
                table: "TopFive",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Residuo",
                table: "OrdensCompra",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataDesativacao",
                table: "TopFive");

            migrationBuilder.DropColumn(
                name: "Residuo",
                table: "OrdensCompra");
        }
    }
}
