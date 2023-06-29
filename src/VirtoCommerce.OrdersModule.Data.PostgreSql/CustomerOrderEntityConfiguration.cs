using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.PostgreSql
{
    public class CustomerOrderEntityConfiguration : IEntityTypeConfiguration<CustomerOrderEntity>
    {
        public void Configure(EntityTypeBuilder<CustomerOrderEntity> builder)
        {
            var converter = new ValueConverter<byte[], long>(
                v => BitConverter.ToInt64(v, 0),
                v => BitConverter.GetBytes(v));

            builder.Property(c => c.RowVersion)
                .HasColumnType("xid")
                .HasColumnName("xmin")
                .HasConversion(converter)
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}
