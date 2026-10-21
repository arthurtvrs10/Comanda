using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoPedidoComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "codigo_pedido",
                table: "comanda",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "uq_comanda_codigo_pedido",
                table: "comanda",
                column: "codigo_pedido",
                unique: true,
                filter: "codigo_pedido IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_comanda_codigo_pedido_valido",
                table: "comanda",
                sql: "codigo_pedido IS NULL OR codigo_pedido BETWEEN 1 AND 999999999");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_comanda_codigo_pedido",
                table: "comanda");

            migrationBuilder.DropCheckConstraint(
                name: "CK_comanda_codigo_pedido_valido",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "codigo_pedido",
                table: "comanda");
        }
    }
}
