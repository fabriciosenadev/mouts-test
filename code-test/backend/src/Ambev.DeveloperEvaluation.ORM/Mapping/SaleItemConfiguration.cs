using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(item => item.ProductId).IsRequired();
        builder.Property(item => item.ProductName).IsRequired().HasMaxLength(150);
        builder.Property(item => item.Quantity).IsRequired();
        builder.Property(item => item.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(item => item.DiscountPercentage).HasColumnType("numeric(5,2)");
        builder.Property(item => item.DiscountAmount).HasColumnType("numeric(18,2)");
        builder.Property(item => item.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(item => item.Cancelled).IsRequired();
    }
}
