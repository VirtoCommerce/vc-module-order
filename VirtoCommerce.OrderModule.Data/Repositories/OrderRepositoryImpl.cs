﻿using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.OrderModule.Data.Repositories
{
    public class OrderRepositoryImpl : EFRepositoryBase, IOrderRepository
    {
        public OrderRepositoryImpl()
        {
        }

        public OrderRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public OrderRepositoryImpl(DbConnection existingConnection, IUnitOfWork unitOfWork = null, IInterceptor[] interceptors = null)
            : base(existingConnection, unitOfWork, interceptors)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            #region Operation
            modelBuilder.Entity<OperationEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);
            modelBuilder.Entity<OperationEntity>().ToTable("OrderOperation");
            #endregion

            #region CustomerOrder
            modelBuilder.Entity<CustomerOrderEntity>().HasKey(x => x.Id)
                    .Property(x => x.Id);
            modelBuilder.Entity<CustomerOrderEntity>().Property(x => x.TaxPercentRate).HasPrecision(18, 4);
            modelBuilder.Entity<CustomerOrderEntity>().ToTable("CustomerOrder");
            #endregion

            #region LineItem
            modelBuilder.Entity<LineItemEntity>().HasKey(x => x.Id)
                    .Property(x => x.Id);

            modelBuilder.Entity<LineItemEntity>().Property(x => x.TaxPercentRate).HasPrecision(18, 4);
            modelBuilder.Entity<LineItemEntity>().HasOptional(x => x.CustomerOrder)
                                       .WithMany(x => x.Items)
                                       .HasForeignKey(x => x.CustomerOrderId).WillCascadeOnDelete(true);


            modelBuilder.Entity<LineItemEntity>().ToTable("OrderLineItem");
            #endregion

            #region ShipmentItemEntity
            modelBuilder.Entity<ShipmentItemEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);


            modelBuilder.Entity<ShipmentItemEntity>().HasRequired(x => x.LineItem)
                                       .WithMany()
                                       .HasForeignKey(x => x.LineItemId).WillCascadeOnDelete(true);

            modelBuilder.Entity<ShipmentItemEntity>().HasRequired(x => x.Shipment)
                                       .WithMany(x => x.Items)
                                       .HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete(true);

            modelBuilder.Entity<ShipmentItemEntity>().HasOptional(x => x.ShipmentPackage)
                                       .WithMany(x => x.Items)
                                       .HasForeignKey(x => x.ShipmentPackageId).WillCascadeOnDelete(true);


            modelBuilder.Entity<ShipmentItemEntity>().ToTable("OrderShipmentItem");
            #endregion

            #region ShipmentPackageEntity
            modelBuilder.Entity<ShipmentPackageEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);


            modelBuilder.Entity<ShipmentPackageEntity>().HasRequired(x => x.Shipment)
                                       .WithMany(x => x.Packages)
                                       .HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete(true);


            modelBuilder.Entity<ShipmentPackageEntity>().ToTable("OrderShipmentPackage");
            #endregion

            #region Shipment
            modelBuilder.Entity<ShipmentEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);

            modelBuilder.Entity<ShipmentEntity>().Property(x => x.TaxPercentRate).HasPrecision(18, 4);
            modelBuilder.Entity<ShipmentEntity>().HasRequired(x => x.CustomerOrder)
                                           .WithMany(x => x.Shipments)
                                           .HasForeignKey(x => x.CustomerOrderId).WillCascadeOnDelete(true);



            modelBuilder.Entity<ShipmentEntity>().ToTable("OrderShipment");
            #endregion

            #region Address
            modelBuilder.Entity<AddressEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);

            modelBuilder.Entity<AddressEntity>().HasOptional(x => x.CustomerOrder)
                                       .WithMany(x => x.Addresses)
                                       .HasForeignKey(x => x.CustomerOrderId).WillCascadeOnDelete(true);


            modelBuilder.Entity<AddressEntity>().HasOptional(x => x.Shipment)
                                       .WithMany(x => x.Addresses)
                                       .HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete(true);


            modelBuilder.Entity<AddressEntity>().HasOptional(x => x.PaymentIn)
                                       .WithMany(x => x.Addresses)
                                       .HasForeignKey(x => x.PaymentInId).WillCascadeOnDelete(true);


            modelBuilder.Entity<AddressEntity>().ToTable("OrderAddress");
            #endregion

            #region PaymentIn
            modelBuilder.Entity<PaymentInEntity>().HasKey(x => x.Id)
                .Property(x => x.Id);

            modelBuilder.Entity<PaymentInEntity>().Property(x => x.TaxPercentRate).HasPrecision(18, 4);
            modelBuilder.Entity<PaymentInEntity>().HasOptional(x => x.CustomerOrder)
                                       .WithMany(x => x.InPayments)
                                       .HasForeignKey(x => x.CustomerOrderId).WillCascadeOnDelete(true);


            modelBuilder.Entity<PaymentInEntity>().HasOptional(x => x.Shipment)
                                       .WithMany(x => x.InPayments)
                                       .HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete(true);


            modelBuilder.Entity<PaymentInEntity>().ToTable("OrderPaymentIn");
            #endregion

            #region Discount
            modelBuilder.Entity<DiscountEntity>().HasKey(x => x.Id)
                        .Property(x => x.Id);


            modelBuilder.Entity<DiscountEntity>().HasOptional(x => x.CustomerOrder)
                                       .WithMany(x => x.Discounts)
                                       .HasForeignKey(x => x.CustomerOrderId).WillCascadeOnDelete(true);

            modelBuilder.Entity<DiscountEntity>().HasOptional(x => x.Shipment)
                                       .WithMany(x => x.Discounts)
                                       .HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete(true);

            modelBuilder.Entity<DiscountEntity>().HasOptional(x => x.LineItem)
                                       .WithMany(x => x.Discounts)
                                       .HasForeignKey(x => x.LineItemId).WillCascadeOnDelete(true);

            modelBuilder.Entity<DiscountEntity>().HasOptional(x => x.PaymentIn)
                                       .WithMany(x => x.Discounts)
                                       .HasForeignKey(x => x.PaymentInId).WillCascadeOnDelete(true);


            modelBuilder.Entity<DiscountEntity>().ToTable("OrderDiscount");
            #endregion

            #region TaxDetail
            modelBuilder.Entity<TaxDetailEntity>().HasKey(x => x.Id)
                        .Property(x => x.Id);


            modelBuilder.Entity<TaxDetailEntity>().HasOptional(x => x.CustomerOrder)
                                       .WithMany(x => x.TaxDetails)
                                       .HasForeignKey(x => x.CustomerOrderId).WillCascadeOnDelete(true);

            modelBuilder.Entity<TaxDetailEntity>().HasOptional(x => x.Shipment)
                                       .WithMany(x => x.TaxDetails)
                                       .HasForeignKey(x => x.ShipmentId).WillCascadeOnDelete(true);

            modelBuilder.Entity<TaxDetailEntity>().HasOptional(x => x.LineItem)
                                       .WithMany(x => x.TaxDetails)
                                       .HasForeignKey(x => x.LineItemId).WillCascadeOnDelete(true);

            modelBuilder.Entity<TaxDetailEntity>().HasOptional(x => x.PaymentIn)
                                   .WithMany(x => x.TaxDetails)
                                   .HasForeignKey(x => x.PaymentInId).WillCascadeOnDelete(true);

            modelBuilder.Entity<TaxDetailEntity>().ToTable("OrderTaxDetail");
            #endregion

            #region PaymentGatewayTransactionEntity
            modelBuilder.Entity<PaymentGatewayTransactionEntity>().HasKey(x => x.Id)
                        .Property(x => x.Id);

            modelBuilder.Entity<PaymentGatewayTransactionEntity>().HasRequired(x => x.PaymentIn)
                                    .WithMany(x => x.Transactions)
                                    .HasForeignKey(x => x.PaymentInId).WillCascadeOnDelete(true);
            modelBuilder.Entity<PaymentGatewayTransactionEntity>().ToTable("OrderPaymentGatewayTransaction");
            #endregion

            base.OnModelCreating(modelBuilder);
        }


        public IQueryable<CustomerOrderEntity> CustomerOrders => GetAsQueryable<CustomerOrderEntity>();
        public IQueryable<ShipmentEntity> Shipments => GetAsQueryable<ShipmentEntity>();
        public IQueryable<PaymentInEntity> InPayments => GetAsQueryable<PaymentInEntity>();
        public IQueryable<AddressEntity> Addresses => GetAsQueryable<AddressEntity>();
        public IQueryable<LineItemEntity> LineItems => GetAsQueryable<LineItemEntity>();
        public IQueryable<PaymentGatewayTransactionEntity> Transactions => GetAsQueryable<PaymentGatewayTransactionEntity>();

        public virtual CustomerOrderEntity[] GetCustomerOrdersByIds(string[] ids, CustomerOrderResponseGroup responseGroup)
        {
            var query = CustomerOrders.Where(x => ids.Contains(x.Id))
                                      .Include(x => x.Discounts)
                                      .Include(x => x.TaxDetails);

            if ((responseGroup & CustomerOrderResponseGroup.WithAddresses) == CustomerOrderResponseGroup.WithAddresses)
            {
                var addresses = Addresses.Where(x => ids.Contains(x.CustomerOrderId)).ToArray();
            }

            if ((responseGroup & CustomerOrderResponseGroup.WithInPayments) == CustomerOrderResponseGroup.WithInPayments)
            {
                var inPayments = InPayments.Include(x => x.TaxDetails)
                                           .Include(x => x.Discounts)
                                           .Where(x => ids.Contains(x.CustomerOrderId)).ToArray();
                var paymentsIds = inPayments.Select(x => x.Id).ToArray();
                var paymentAddresses = Addresses.Where(x => paymentsIds.Contains(x.PaymentInId)).ToArray();
                var transactions = Transactions.Where(x => paymentsIds.Contains(x.PaymentInId)).ToArray();
            }

            if ((responseGroup & CustomerOrderResponseGroup.WithItems) == CustomerOrderResponseGroup.WithItems)
            {
                var lineItems = LineItems.Include(x => x.TaxDetails)
                                         .Include(x => x.Discounts)
                                         .Where(x => ids.Contains(x.CustomerOrderId))
                                         .OrderByDescending(x => x.CreatedDate).ToArray();
            }

            if ((responseGroup & CustomerOrderResponseGroup.WithShipments) == CustomerOrderResponseGroup.WithShipments)
            {
                var shipments = Shipments.Include(x => x.TaxDetails)
                                         .Include(x => x.Discounts)
                                         .Include(x => x.Items)
                                         .Include(x => x.Packages.Select(y => y.Items))
                                         .Where(x => ids.Contains(x.CustomerOrderId)).ToArray();
                var shipmentIds = shipments.Select(x => x.Id).ToArray();
                var addresses = Addresses.Where(x => shipmentIds.Contains(x.ShipmentId)).ToArray();
            }

            return query.ToArray();
        }

        public virtual void RemoveOrdersByIds(string[] ids)
        {
            var orders = GetCustomerOrdersByIds(ids, CustomerOrderResponseGroup.Full);
            foreach (var order in orders)
            {
                Remove(order);
            }
        }
    }
}
