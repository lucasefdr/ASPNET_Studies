using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Studies.Domain.Entities;

namespace Studies.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("TB_PRODUCT");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .IsRequired()
            .HasColumnName("ID");

        builder.Property(p => p.Description)
            .IsRequired()
            .HasColumnName("DESCRIPTION")
            .HasMaxLength(256);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnName("PRICE")
            .HasColumnType("decimal(18,2)");
    }
}
