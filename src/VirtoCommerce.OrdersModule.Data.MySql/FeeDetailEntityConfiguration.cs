using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class FeeDetailEntityConfiguration : IEntityTypeConfiguration<FeeDetailEntity>
    {
        public void Configure(EntityTypeBuilder<FeeDetailEntity> builder)
        {
            builder.Property(x => x.Amount).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
