using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class RefundEntityConfiguration : IEntityTypeConfiguration<RefundEntity>
    {
        public void Configure(EntityTypeBuilder<RefundEntity> builder)
        {
            builder.Property(x => x.Amount).HasColumnType("decimal").HasPrecision(18, 4);

            builder.Property(x => x.Sum).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
