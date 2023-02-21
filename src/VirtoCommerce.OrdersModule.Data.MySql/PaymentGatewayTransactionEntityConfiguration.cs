using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.MySql
{
    public class PaymentGatewayTransactionEntityConfiguration : IEntityTypeConfiguration<PaymentGatewayTransactionEntity>
    {
        public void Configure(EntityTypeBuilder<PaymentGatewayTransactionEntity> builder)
        {
            builder.Property(x => x.Amount).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
