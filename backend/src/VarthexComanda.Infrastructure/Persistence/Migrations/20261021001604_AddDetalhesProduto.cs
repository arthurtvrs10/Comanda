using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDetalhesProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_personalizado",
                table: "produto",
                type: "TEXT",
                maxLength: 40,
                nullable: true,
                collation: "NOCASE");

            migrationBuilder.AddColumn<long>(
                name: "custo_centavos",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                table: "produto",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_produto_codigo_personalizado",
                table: "produto",
                column: "codigo_personalizado",
                unique: true,
                filter: "codigo_personalizado IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_produto_custo_nao_negativo",
                table: "produto",
                sql: "custo_centavos >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_produto_codigo_personalizado",
                table: "produto");

            migrationBuilder.DropCheckConstraint(
                name: "CK_produto_custo_nao_negativo",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "codigo_personalizado",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "custo_centavos",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "descricao",
                table: "produto");
        }
    }
}
