using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrdersModule.Data.Repositories
{
    public class OrderDbContext : DbContextBase
    {
#pragma warning disable S109
        private const int MaxLength = 128;
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        protected OrderDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region CustomerOrder

            modelBuilder.Entity<CustomerOrderEntity>().ToTable("CustomerOrder").HasKey(x => x.Id);
            modelBuilder.Entity<CustomerOrderEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();
            modelBuilder.Entity<CustomerOrderEntity>().Property(x => x.TaxPercentRate).HasColumnType("decimal(18,4)");

            #endregion

            #region LineItem

            modelBuilder.Entity<LineItemEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<LineItemEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();
            modelBuilder.Entity<LineItemEntity>().Property(x => x.Weight).HasPrecision(18, 4);
            modelBuilder.Entity<LineItemEntity>().Property(x => x.Height).HasPrecision(18, 4);
            modelBuilder.Entity<LineItemEntity>().Property(x => x.Length).HasPrecision(18, 4);
            modelBuilder.Entity<LineItemEntity>().Property(x => x.Width).HasPrecision(18, 4);
            modelBuilder.Entity<LineItemEntity>().Property(x => x.TaxPercentRate).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<LineItemEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.Items)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LineItemEntity>().ToTable("OrderLineItem");
            #endregion

            #region ShipmentItemEntity

            modelBuilder.Entity<ShipmentItemEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<ShipmentItemEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<ShipmentItemEntity>().HasOne(x => x.LineItem).WithMany(x => x.ShipmentItems)
                        .HasForeignKey(x => x.LineItemId).OnDelete(DeleteBehavior.Cascade).IsRequired();

            modelBuilder.Entity<ShipmentItemEntity>().HasOne(x => x.Shipment).WithMany(x => x.Items)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade).IsRequired();

            modelBuilder.Entity<ShipmentItemEntity>().HasOne(x => x.ShipmentPackage).WithMany(x => x.Items)
                        .HasForeignKey(x => x.ShipmentPackageId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShipmentItemEntity>().ToTable("OrderShipmentItem");
            #endregion

            #region ShipmentPackageEntity

            modelBuilder.Entity<ShipmentPackageEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<ShipmentPackageEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();
            modelBuilder.Entity<ShipmentPackageEntity>().Property(x => x.Weight).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentPackageEntity>().Property(x => x.Height).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentPackageEntity>().Property(x => x.Length).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentPackageEntity>().Property(x => x.Width).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentPackageEntity>().HasOne(x => x.Shipment).WithMany(x => x.Packages)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade).IsRequired();


            modelBuilder.Entity<ShipmentPackageEntity>().ToTable("OrderShipmentPackage");
            #endregion

            #region Shipment

            modelBuilder.Entity<ShipmentEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.VolumetricWeight).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.Weight).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.Height).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.Length).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.Width).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentEntity>().Property(x => x.TaxPercentRate).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<ShipmentEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.Shipments)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade).IsRequired();
            modelBuilder.Entity<ShipmentEntity>().ToTable("OrderShipment");

            #endregion

            #region Address

            modelBuilder.Entity<AddressEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<AddressEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<AddressEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.Addresses)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AddressEntity>().HasOne(x => x.Shipment).WithMany(x => x.Addresses)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AddressEntity>().HasOne(x => x.PaymentIn).WithMany(x => x.Addresses)
                        .HasForeignKey(x => x.PaymentInId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AddressEntity>().ToTable("OrderAddress");
            #endregion

            #region PaymentIn

            modelBuilder.Entity<PaymentInEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<PaymentInEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<PaymentInEntity>().Property(x => x.TaxPercentRate).HasColumnType("decimal(18,4)");
            modelBuilder.Entity<PaymentInEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.InPayments)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentInEntity>().HasOne(x => x.Shipment).WithMany(x => x.InPayments)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentInEntity>().ToTable("OrderPaymentIn");

            #endregion

            #region Refund

            modelBuilder.Entity<RefundEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<RefundEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<RefundEntity>().HasOne(x => x.CustomerOrder).WithMany()
                .HasForeignKey(x => x.CustomerOrderId);

            modelBuilder.Entity<RefundEntity>().HasOne(x => x.Payment).WithMany(x => x.Refunds)
                .HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefundEntity>().HasIndex(x => new { x.TransactionId, x.CustomerOrderId }).IsUnique();

            modelBuilder.Entity<RefundEntity>().ToTable("OrderRefund");

            #endregion

            #region RefundItem

            modelBuilder.Entity<RefundItemEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<RefundItemEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<RefundItemEntity>().HasOne(x => x.LineItem).WithMany(x => x.RefundItems)
                        .HasForeignKey(x => x.LineItemId).OnDelete(DeleteBehavior.Cascade).IsRequired();

            modelBuilder.Entity<RefundItemEntity>().HasOne(x => x.Refund).WithMany(x => x.Items)
                        .HasForeignKey(x => x.RefundId).OnDelete(DeleteBehavior.Restrict).IsRequired();

            modelBuilder.Entity<RefundItemEntity>().ToTable("OrderRefundItem");

            #endregion

            #region Capture

            modelBuilder.Entity<CaptureEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<CaptureEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<CaptureEntity>().HasOne(x => x.CustomerOrder).WithMany()
                .HasForeignKey(x => x.CustomerOrderId);

            modelBuilder.Entity<CaptureEntity>().HasOne(x => x.Payment).WithMany(x => x.Captures)
                .HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CaptureEntity>().HasIndex(x => new { x.TransactionId, x.CustomerOrderId }).IsUnique();

            modelBuilder.Entity<CaptureEntity>().ToTable("OrderCapture");

            #endregion

            #region CaptureItem

            modelBuilder.Entity<CaptureItemEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<CaptureItemEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<CaptureItemEntity>().HasOne(x => x.LineItem).WithMany(x => x.CaptureItems)
                .HasForeignKey(x => x.LineItemId).OnDelete(DeleteBehavior.Cascade).IsRequired();

            modelBuilder.Entity<CaptureItemEntity>().HasOne(x => x.Capture).WithMany(x => x.Items)
                .HasForeignKey(x => x.CaptureId).OnDelete(DeleteBehavior.Restrict).IsRequired();

            modelBuilder.Entity<CaptureItemEntity>().ToTable("OrderCaptureItem");

            #endregion

            #region Discount

            modelBuilder.Entity<DiscountEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<DiscountEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<DiscountEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.Discounts)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiscountEntity>().HasOne(x => x.Shipment).WithMany(x => x.Discounts)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiscountEntity>().HasOne(x => x.LineItem).WithMany(x => x.Discounts)
                        .HasForeignKey(x => x.LineItemId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiscountEntity>().HasOne(x => x.PaymentIn).WithMany(x => x.Discounts)
                        .HasForeignKey(x => x.PaymentInId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DiscountEntity>().ToTable("OrderDiscount");
            #endregion

            #region TaxDetail

            modelBuilder.Entity<TaxDetailEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<TaxDetailEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<TaxDetailEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.TaxDetails)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaxDetailEntity>().HasOne(x => x.Shipment).WithMany(x => x.TaxDetails)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaxDetailEntity>().HasOne(x => x.LineItem).WithMany(x => x.TaxDetails)
                        .HasForeignKey(x => x.LineItemId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaxDetailEntity>().HasOne(x => x.PaymentIn).WithMany(x => x.TaxDetails)
                        .HasForeignKey(x => x.PaymentInId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaxDetailEntity>().Property(x => x.Rate).HasPrecision(18, 2);
            modelBuilder.Entity<TaxDetailEntity>().Property(x => x.Rate).HasPrecision(18, 2);

            modelBuilder.Entity<TaxDetailEntity>().ToTable("OrderTaxDetail");
            #endregion

            #region FeeDetail

            modelBuilder.Entity<FeeDetailEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<FeeDetailEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<FeeDetailEntity>().HasOne(x => x.CustomerOrder).WithMany(x => x.FeeDetails)
                        .HasForeignKey(x => x.CustomerOrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeeDetailEntity>().HasOne(x => x.Shipment).WithMany(x => x.FeeDetails)
                        .HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeeDetailEntity>().HasOne(x => x.LineItem).WithMany(x => x.FeeDetails)
                        .HasForeignKey(x => x.LineItemId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeeDetailEntity>().HasOne(x => x.PaymentIn).WithMany(x => x.FeeDetails)
                        .HasForeignKey(x => x.PaymentInId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FeeDetailEntity>().ToTable("OrderFeeDetail");

            #endregion

            #region PaymentGatewayTransactionEntity

            modelBuilder.Entity<PaymentGatewayTransactionEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<PaymentGatewayTransactionEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();

            modelBuilder.Entity<PaymentGatewayTransactionEntity>().HasOne(x => x.PaymentIn).WithMany(x => x.Transactions)
                        .HasForeignKey(x => x.PaymentInId).OnDelete(DeleteBehavior.Cascade).IsRequired();
            modelBuilder.Entity<PaymentGatewayTransactionEntity>().ToTable("OrderPaymentGatewayTransaction");
            #endregion

            #region DynamicPropertyValues

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().ToTable("OrderDynamicPropertyObjectValue").HasKey(x => x.Id);
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().Property(x => x.DecimalValue).HasColumnType("decimal(18,5)");

            //need to set DeleteBehavior.Cascade manually
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasOne(p => p.CustomerOrder)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.CustomerOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            //need to set DeleteBehavior.Cascade manually
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasOne(p => p.PaymentIn)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.PaymentInId)
                .OnDelete(DeleteBehavior.Cascade);

            //need to set DeleteBehavior.Cascade manually
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasOne(p => p.Shipment)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            //need to set DeleteBehavior.Cascade manually
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasOne(p => p.Refund)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.RefundId)
                .OnDelete(DeleteBehavior.Cascade);

            //need to set DeleteBehavior.Cascade manually
            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasOne(p => p.LineItem)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.LineItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.CustomerOrderId })
                .IsUnique(false)
                .HasDatabaseName("IX_OrderDynamicProperty_ObjectType_CustomerOrderId");

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.PaymentInId })
                .IsUnique(false)
                .HasDatabaseName("IX_OrderDynamicProperty_ObjectType_PaymentInId");

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.ShipmentId })
                .IsUnique(false)
                .HasDatabaseName("IX_OrderDynamicProperty_ObjectType_ShipmentId");

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.RefundId })
                .IsUnique(false)
                .HasDatabaseName("IX_OrderDynamicProperty_ObjectType_RefundId");

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.LineItemId })
                .IsUnique(false)
                .HasDatabaseName("IX_OrderDynamicProperty_ObjectType_LineItemId");

            modelBuilder.Entity<OrderDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.ObjectId })
                .IsUnique(false)
                .HasDatabaseName("IX_OrderDynamicProperty_ObjectType_ObjectId");

            #endregion

            #region ConfigurationItem

            modelBuilder.Entity<ConfigurationItemEntity>().ToTable("OrderConfigurationItem").HasKey(x => x.Id);
            modelBuilder.Entity<ConfigurationItemEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ConfigurationItemEntity>().HasOne(x => x.LineItem).WithMany(x => x.ConfigurationItems)
                        .HasForeignKey(x => x.LineItemId).IsRequired().OnDelete(DeleteBehavior.ClientCascade);

            #endregion

            #region ConfigurationItemImage

            modelBuilder.Entity<ConfigurationItemFileEntity>().ToTable("OrderConfigurationItemFile").HasKey(x => x.Id);
            modelBuilder.Entity<ConfigurationItemFileEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<ConfigurationItemFileEntity>().HasOne(x => x.ConfigurationItem).WithMany(x => x.Files)
                .HasForeignKey(x => x.ConfigurationItemId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            #endregion ConfigurationItemImage

            #region PurchasedProduct

            modelBuilder.Entity<PurchasedProductEntity>().ToTable("PurchasedProduct").HasKey(x => x.Id);
            modelBuilder.Entity<PurchasedProductEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

            #endregion 

            base.OnModelCreating(modelBuilder);

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.OrdersModule.Data.XXX project. /> 
            switch (this.Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.OrdersModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.OrdersModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.OrdersModule.Data.SqlServer"));
                    break;
            }
        }
    }
#pragma warning restore S109
}
