using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace VarthexComanda.Infrastructure.Persistence.Migrations;
public partial class AddUnidadesVenda : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "unidade_venda", table: "produto", type: "INTEGER", nullable: false, defaultValue: 0);
        // Rebuild atomically: whole legacy units become exact thousandths, preserving IDs and totals.
        migrationBuilder.Sql("""
            ALTER TABLE item_comanda RENAME TO item_comanda_unidades_legado;
            CREATE TABLE item_comanda (
                id INTEGER NOT NULL CONSTRAINT PK_item_comanda PRIMARY KEY AUTOINCREMENT,
                comanda_id INTEGER NOT NULL, produto_id INTEGER NOT NULL, nome_produto TEXT NOT NULL,
                preco_unitario_centavos INTEGER NOT NULL, quantidade INTEGER NOT NULL,
                unidade_venda INTEGER NOT NULL DEFAULT 0, subtotal_centavos INTEGER NOT NULL,
                observacao TEXT NULL, lancado_por TEXT NULL,
                quantidade_cozinha INTEGER NOT NULL DEFAULT 0, quantidade_ficha INTEGER NOT NULL DEFAULT 0,
                criado_em TEXT NOT NULL, atualizado_em TEXT NOT NULL,
                CONSTRAINT FK_item_comanda_comanda_comanda_id FOREIGN KEY (comanda_id) REFERENCES comanda (id) ON DELETE RESTRICT,
                CONSTRAINT FK_item_comanda_produto_produto_id FOREIGN KEY (produto_id) REFERENCES produto (id) ON DELETE RESTRICT,
                CONSTRAINT CK_item_comanda_preco_unitario_positivo CHECK (preco_unitario_centavos > 0),
                CONSTRAINT CK_item_comanda_quantidade_positiva CHECK (quantidade > 0),
                CONSTRAINT CK_item_comanda_subtotal_positivo CHECK (subtotal_centavos > 0),
                CONSTRAINT CK_item_comanda_subtotal_igual_preco_vezes_quantidade CHECK (subtotal_centavos = CAST((preco_unitario_centavos * quantidade + 500) / 1000 AS INTEGER)),
                CONSTRAINT CK_item_comanda_unidade_valida CHECK (unidade_venda IN (0,1,2) AND (unidade_venda <> 0 OR quantidade % 1000 = 0))
            );
            INSERT INTO item_comanda (id, comanda_id, produto_id, nome_produto, preco_unitario_centavos, quantidade, subtotal_centavos, observacao, lancado_por, quantidade_cozinha, quantidade_ficha, criado_em, atualizado_em)
            SELECT id, comanda_id, produto_id, nome_produto, preco_unitario_centavos, quantidade * 1000, subtotal_centavos, observacao, lancado_por, quantidade_cozinha * 1000, quantidade_ficha * 1000, criado_em, atualizado_em FROM item_comanda_unidades_legado;
            DROP TABLE item_comanda_unidades_legado;
            CREATE INDEX idx_item_comanda_comanda ON item_comanda (comanda_id);
            CREATE INDEX IX_item_comanda_produto_id ON item_comanda (produto_id);
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Quantidades fracionadas não podem ser convertidas para a versão anterior. Restaure um backup anterior à atualização.");
}
