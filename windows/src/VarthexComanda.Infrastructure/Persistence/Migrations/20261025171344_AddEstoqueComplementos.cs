using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoqueComplementos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "categoria_id",
                table: "produto",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<bool>(
                name: "controlar_estoque",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "quantidade_estoque",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "versao_estoque",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "estoque_reservado_json",
                table: "item_comanda",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddCheckConstraint(
                name: "CK_produto_categoria_obrigatoria",
                table: "produto",
                sql: "eh_complemento = 1 OR categoria_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_produto_estoque_nao_negativo",
                table: "produto",
                sql: "quantidade_estoque >= 0");
            migrationBuilder.Sql("UPDATE produto SET categoria_id = NULL WHERE eh_complemento = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_produto_categoria_obrigatoria",
                table: "produto");

            migrationBuilder.DropCheckConstraint(
                name: "CK_produto_estoque_nao_negativo",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "controlar_estoque",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "quantidade_estoque",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "versao_estoque",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "estoque_reservado_json",
                table: "item_comanda");

            migrationBuilder.AlterColumn<int>(
                name: "categoria_id",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
