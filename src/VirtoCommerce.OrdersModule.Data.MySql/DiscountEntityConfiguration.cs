using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class DiscountEntityConfiguration : IEntityTypeConfiguration<DiscountEntity>
    {
        public void Configure(EntityTypeBuilder<DiscountEntity> builder)
        {
            builder.Property(x => x.DiscountAmount).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.DiscountAmountWithTax).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
