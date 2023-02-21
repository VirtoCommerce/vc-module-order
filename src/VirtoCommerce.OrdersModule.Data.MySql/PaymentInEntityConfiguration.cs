using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class PaymentInEntityConfiguration : IEntityTypeConfiguration<PaymentInEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentInEntity> builder)
        {
            builder.Property(x => x.Price).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.PriceWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.DiscountAmountWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.Total).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.TotalWithTax).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.TaxTotal).HasColumnType("decimal").HasPrecision(18, 4);

            builder.Property(x => x.Sum).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
