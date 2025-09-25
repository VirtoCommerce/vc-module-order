using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.OrdersModule.Data.Model;

namespace VirtoCommerce.OrdersModule.Data.SqlServer;

public class CustomerOrderEntityConfiguration : IEntityTypeConfiguration<CustomerOrderEntity>
{
    public void Configure(EntityTypeBuilder<CustomerOrderEntity> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
    }
}

public class ShipmentEntityConfiguration : IEntityTypeConfiguration<ShipmentEntity>
{
    public void Configure(EntityTypeBuilder<ShipmentEntity> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
    }
}

public class PaymentInEntityConfiguration : IEntityTypeConfiguration<PaymentInEntity>
{
    public void Configure(EntityTypeBuilder<PaymentInEntity> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
    }
}

public class AddressEntityConfiguration : IEntityTypeConfiguration<AddressEntity>
{
    public void Configure(EntityTypeBuilder<AddressEntity> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
    }
}

public class LineItemEntityConfiguration : IEntityTypeConfiguration<LineItemEntity>
{
    public void Configure(EntityTypeBuilder<LineItemEntity> builder)
    {
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
    }
}

