using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VarthexComanda.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoComplementos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_produto_preco_centavos_positivo",
                table: "produto");

            migrationBuilder.AddColumn<bool>(
                name: "eh_complemento",
                table: "produto",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "origem_complemento_id",
                table: "produto",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_produto_origem_complemento_id",
                table: "produto",
                column: "origem_complemento_id",
                unique: true,
                filter: "origem_complemento_id IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_produto_preco_centavos_positivo",
                table: "produto",
                sql: "preco_centavos > 0 OR (eh_complemento = 1 AND preco_centavos = 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_produto_origem_complemento_id",
                table: "produto");

            migrationBuilder.DropCheckConstraint(
                name: "CK_produto_preco_centavos_positivo",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "eh_complemento",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "origem_complemento_id",
                table: "produto");

            migrationBuilder.AddCheckConstraint(
                name: "CK_produto_preco_centavos_positivo",
                table: "produto",
                sql: "preco_centavos > 0");
        }
    }
}
