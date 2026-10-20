using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMesaComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "numero_mesa",
                table: "comanda",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_comanda_mesa_valida",
                table: "comanda",
                sql: "numero_mesa IS NULL OR numero_mesa BETWEEN 1 AND 999");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_comanda_mesa_valida",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "numero_mesa",
                table: "comanda");
        }
    }
}
