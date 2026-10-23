using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusPreparoCozinha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status_preparo",
                table: "item_comanda",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_item_comanda_preparo_valido",
                table: "item_comanda",
                sql: "status_preparo IN (0,1,2,3)");

            // Não reenviar vendas antigas à cozinha; comandas abertas continuam pendentes para conferência.
            migrationBuilder.Sql("UPDATE item_comanda SET status_preparo = 3 WHERE comanda_id IN (SELECT id FROM comanda WHERE status <> 'ABERTA');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_item_comanda_preparo_valido",
                table: "item_comanda");

            migrationBuilder.DropColumn(
                name: "status_preparo",
                table: "item_comanda");
        }
    }
}
