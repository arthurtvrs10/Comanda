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
            t.HasCheckConstraint("CK_produto_preco_centavos_positivo", "preco_centavos > 0");
            t.HasCheckConstraint("CK_produto_ativo", "ativo IN (0, 1)");
            t.HasCheckConstraint("CK_produto_custo_nao_negativo", "custo_centavos >= 0");
        });
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.CategoriaId).HasColumnName("categoria_id").IsRequired();
        builder.Property(p => p.Nome).HasColumnName("nome").IsRequired();
        builder.Property(p => p.PrecoCentavos).HasColumnName("preco_centavos").IsRequired();
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
