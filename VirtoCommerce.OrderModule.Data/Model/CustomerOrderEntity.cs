using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
	public class CustomerOrderEntity : OperationEntity
	{
		public CustomerOrderEntity()
		{
			Addresses = new NullCollection<AddressEntity>();
			InPayments = new NullCollection<PaymentInEntity>();
			Items = new NullCollection<LineItemEntity>();
			Shipments = new NullCollection<ShipmentEntity>();
			Discounts = new NullCollection<DiscountEntity>();
			TaxDetails = new NullCollection<TaxDetailEntity>();
		}

		[Required]
		[StringLength(64)]
		public string CustomerId { get; set; }
		[StringLength(255)]
		public string CustomerName { get; set; }

		[Required]
		[StringLength(64)]
		public string StoreId { get; set; }
		[StringLength(255)]
		public string StoreName { get; set; }

		[StringLength(64)]
		public string ChannelId { get; set; }
		[StringLength(64)]
		public string OrganizationId { get; set; }
		[StringLength(255)]
		public string OrganizationName { get; set; }

		[StringLength(64)]
		public string EmployeeId { get; set; }
		[StringLength(255)]
		public string EmployeeName { get; set; }


		public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; }
		public virtual ObservableCollection<AddressEntity> Addresses { get; set; }
		public virtual ObservableCollection<PaymentInEntity> InPayments { get; set; }

		public virtual ObservableCollection<LineItemEntity> Items { get; set; }
		public virtual ObservableCollection<ShipmentEntity> Shipments { get; set; }

		public virtual ObservableCollection<DiscountEntity> Discounts { get; set; }

        public override OrderOperation ToModel(OrderOperation operation)
        {
            base.ToModel(operation);

            var order = operation as CustomerOrder;
            if (order == null)
                throw new ArgumentNullException("order");

            base.ToModel(order);

            if (!this.Discounts.IsNullOrEmpty())
            {
                order.Discount = this.Discounts.First().ToModel(AbstractTypeFactory<Discount>.TryCreateInstance());
            }
            if (!this.Items.IsNullOrEmpty())
            {
                order.Items = this.Items.Select(x => x.ToModel(AbstractTypeFactory<LineItem>.TryCreateInstance())).ToList();
            }
            if (!this.Addresses.IsNullOrEmpty())
            {
                order.Addresses = this.Addresses.Select(x => x.ToModel(AbstractTypeFactory<Address>.TryCreateInstance())).ToList();
            }
            if (!this.Shipments.IsNullOrEmpty())
            {
                order.Shipments = this.Shipments.Select(x => x.ToModel(AbstractTypeFactory<Shipment>.TryCreateInstance())).OfType<Shipment>().ToList();
            }
            if (!this.InPayments.IsNullOrEmpty())
            {
                order.InPayments = this.InPayments.Select(x => x.ToModel(AbstractTypeFactory<PaymentIn>.TryCreateInstance())).OfType<PaymentIn>().ToList();
            }
            if(!this.TaxDetails.IsNullOrEmpty())
            {
                order.TaxDetails = this.TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            }
            order.ChildrenOperations = order.GetFlatObjectsListWithInterface<IOperation>().Except(new[] { order }).ToList();

            return order;
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            base.FromModel(operation, pkMap);

            var order = operation as CustomerOrder;
            if (order == null)
                throw new ArgumentNullException("order");
        
            if (order.Addresses != null)
            {
                this.Addresses = new ObservableCollection<AddressEntity>(order.Addresses.Select(x => AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(x)));
            }
            if (order.Items != null)
            {
                this.Items = new ObservableCollection<LineItemEntity>(order.Items.Select(x => AbstractTypeFactory<LineItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }
            if (order.Shipments != null)
            {
                this.Shipments = new ObservableCollection<ShipmentEntity>(order.Shipments.Select(x => AbstractTypeFactory<ShipmentEntity>.TryCreateInstance().FromModel(x, pkMap)).OfType<ShipmentEntity>());
            }
            if (order.InPayments != null)
            {
                this.InPayments = new ObservableCollection<PaymentInEntity>(order.InPayments.Select(x => AbstractTypeFactory<PaymentInEntity>.TryCreateInstance().FromModel(x, pkMap)).OfType<PaymentInEntity>());
            }
            if (order.Discount != null)
            {
                this.Discounts = new ObservableCollection<DiscountEntity>(new DiscountEntity[] { AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(order.Discount) });
            }
            if (order.TaxDetails != null)
            {
                this.TaxDetails = new ObservableCollection<TaxDetailEntity>(order.TaxDetails.Select(x => AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x))); 
            }

            return this;
        }

        public override void Patch(OperationEntity operation)
        {
            base.Patch(operation);

            var target = operation as CustomerOrderEntity;
            if (target == null)
                throw new ArgumentNullException("target");

            target.CustomerId = this.CustomerId;
            target.StoreId = this.StoreId;
            target.OrganizationId = this.OrganizationId;
            target.EmployeeId = this.EmployeeId;
            
            if (!this.Addresses.IsNullCollection())
            {
                this.Addresses.Patch(target.Addresses, new AddressComparer(), (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!this.Shipments.IsNullCollection())
            {
                this.Shipments.Patch(target.Shipments, (sourceShipment, targetShipment) => sourceShipment.Patch(targetShipment));
            }

            if (!this.Items.IsNullCollection())
            {
                this.Items.Patch(target.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!this.InPayments.IsNullCollection())
            {
                this.InPayments.Patch(target.InPayments, (sourcePayment, targetPayment) => sourcePayment.Patch(targetPayment));
            }

            if (!this.Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => x.PromotionId);
                this.Discounts.Patch(target.Discounts, discountComparer, (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }
            if (!this.TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                this.TaxDetails.Patch(target.TaxDetails, taxDetailComparer, (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }
        }
    }
}
