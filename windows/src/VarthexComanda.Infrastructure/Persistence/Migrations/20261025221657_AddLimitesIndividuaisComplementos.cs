using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLimitesIndividuaisComplementos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "maximo_complemento",
                table: "produto",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "minimo_complemento",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "maximo_complemento",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "minimo_complemento",
                table: "produto");
        }
    }
}
