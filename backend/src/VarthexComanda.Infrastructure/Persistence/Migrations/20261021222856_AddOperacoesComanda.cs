using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOperacoesComanda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lancado_por",
                table: "item_comanda",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantidade_cozinha",
                table: "item_comanda",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "quantidade_ficha",
                table: "item_comanda",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "bloqueada",
                table: "comanda",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "criado_por",
                table: "comanda",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "responsavel",
                table: "comanda",
                type: "TEXT",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lancado_por",
                table: "item_comanda");

            migrationBuilder.DropColumn(
                name: "quantidade_cozinha",
                table: "item_comanda");

            migrationBuilder.DropColumn(
                name: "quantidade_ficha",
                table: "item_comanda");

            migrationBuilder.DropColumn(
                name: "bloqueada",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "criado_por",
                table: "comanda");

            migrationBuilder.DropColumn(
                name: "responsavel",
                table: "comanda");
        }
    }
}
