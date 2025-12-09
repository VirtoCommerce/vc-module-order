using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class ConfigurationItemEntityConfiguration : IEntityTypeConfiguration<ConfigurationItemEntity>
    {
        public void Configure(EntityTypeBuilder<ConfigurationItemEntity> builder)
        {
            builder.Property(x => x.Price).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.SalePrice).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
