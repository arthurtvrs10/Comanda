using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VarthexComanda.Domain;

namespace VarthexComanda.Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produto", t =>
        {
            t.HasCheckConstraint("CK_produto_preco_centavos_positivo", "preco_centavos > 0 OR (eh_complemento = 1 AND preco_centavos = 0)");
            t.HasCheckConstraint("CK_produto_estoque_nao_negativo", "quantidade_estoque >= 0");
            t.HasCheckConstraint("CK_produto_categoria_obrigatoria", "eh_complemento = 1 OR categoria_id IS NOT NULL");
            t.HasCheckConstraint("CK_produto_ativo", "ativo IN (0, 1)");
            t.HasCheckConstraint("CK_produto_custo_nao_negativo", "custo_centavos >= 0");
        });
        builder.Ignore(p=>p.EstoqueDescricao);
        builder.Property(p=>p.ControlarEstoque).HasColumnName("controlar_estoque").HasDefaultValue(false);
        builder.Property(p=>p.QuantidadeEstoque).HasColumnName("quantidade_estoque").HasDefaultValue(0L);
        builder.Property(p=>p.VersaoEstoque).HasColumnName("versao_estoque").HasDefaultValue(0L).IsConcurrencyToken();
        builder.Property(p=>p.EhComplemento).HasColumnName("eh_complemento").HasDefaultValue(false);
        builder.Property(p=>p.OrigemComplementoId).HasColumnName("origem_complemento_id");
        builder.HasIndex(p=>p.OrigemComplementoId).IsUnique().HasFilter("origem_complemento_id IS NOT NULL");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p=>p.MinimoComplemento).HasColumnName("minimo_complemento").HasDefaultValue(0);
        builder.Property(p=>p.MaximoComplemento).HasColumnName("maximo_complemento");
        builder.Property(p => p.CategoriaId).HasColumnName("categoria_id").IsRequired(false);
        builder.Property(p => p.Nome).HasColumnName("nome").IsRequired();
        builder.Property(p => p.PrecoCentavos).HasColumnName("preco_centavos").IsRequired();
        builder.Property(p => p.UnidadeVenda).HasColumnName("unidade_venda").HasConversion<int>().HasDefaultValue(UnidadeVenda.Unidade);
        builder.Property(p=>p.ComplementosJson).HasColumnName("complementos_json").HasDefaultValue("[]");
        builder.Ignore(p=>p.GruposComplementos);
        builder.Ignore(p => p.Medida);
        builder.Property(p => p.Ativo).HasColumnName("ativo").HasConversion<int>().IsRequired();
        builder.Property(p => p.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.Property(p => p.AtualizadoEm).HasColumnName("atualizado_em").IsRequired();
        builder.Property(p => p.FotoArquivo).HasColumnName("foto_arquivo");
        builder.Property(p=>p.CustoCentavos).HasColumnName("custo_centavos").HasDefaultValue(0L);
        builder.Property(p=>p.Descricao).HasColumnName("descricao").HasMaxLength(2000);
        builder.Property(p=>p.CodigoPersonalizado).HasColumnName("codigo_personalizado").HasMaxLength(40).UseCollation("NOCASE");
        builder.HasIndex(p=>p.CodigoPersonalizado).IsUnique().HasFilter("codigo_personalizado IS NOT NULL").HasDatabaseName("idx_produto_codigo_personalizado");

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.CategoriaId, p.Ativo }).HasDatabaseName("idx_produto_categoria_ativo");
    }
}
