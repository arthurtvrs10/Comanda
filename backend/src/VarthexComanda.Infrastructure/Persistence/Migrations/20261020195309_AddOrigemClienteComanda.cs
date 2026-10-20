using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrigemClienteComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "origem_cliente",
                table: "comanda",
                type: "TEXT",
                maxLength: 80,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "origem_cliente",
                table: "comanda");
        }
    }
}
