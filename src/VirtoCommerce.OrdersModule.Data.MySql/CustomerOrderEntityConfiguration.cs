using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class CustomerOrderEntityConfiguration : IEntityTypeConfiguration<CustomerOrderEntity>
    {
        public void Configure(EntityTypeBuilder<CustomerOrderEntity> builder)
        {
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.TaxTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.Total).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.SubTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.SubTotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.ShippingTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.ShippingTotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.PaymentTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.PaymentTotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.Fee).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.FeeWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.FeeTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.FeeTotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.HandlingTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.HandlingTotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.DiscountTotal).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.DiscountTotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);

            builder.Property(x => x.Sum).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
