using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrioridadeComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "prioridade",
                table: "comanda",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "prioridade",
                table: "comanda");
        }
    }
}
