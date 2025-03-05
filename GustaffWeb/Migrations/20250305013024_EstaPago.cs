using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GustaffWeb.Migrations
{
    /// <inheritdoc />
    public partial class EstaPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Pago",
                table: "Despesas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pago",
                table: "Despesas");
        }
    }
}
