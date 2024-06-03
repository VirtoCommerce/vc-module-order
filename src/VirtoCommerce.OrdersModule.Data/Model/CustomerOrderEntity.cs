using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CoreModule.Core.Tax;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.DynamicProperties;
using Address = VirtoCommerce.OrdersModule.Core.Model.Address;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class CustomerOrderEntity : OperationEntity, ISupportPartialPriceUpdate, IDataEntity<CustomerOrderEntity, CustomerOrder>
    {
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

        [StringLength(64)]
        public string SubscriptionId { get; set; }
        [StringLength(64)]
        public string SubscriptionNumber { get; set; }

        public bool IsPrototype { get; set; }

        [Column(TypeName = "Money")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "Money")]
        public decimal TaxTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal Total { get; set; }
        [Column(TypeName = "Money")]
        public decimal SubTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal SubTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal ShippingTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal ShippingTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal PaymentTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal PaymentTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal Fee { get; set; }
        [Column(TypeName = "Money")]
        public decimal FeeWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal FeeTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal FeeTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal HandlingTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal HandlingTotalWithTax { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountTotal { get; set; }
        [Column(TypeName = "Money")]
        public decimal DiscountTotalWithTax { get; set; }
        [StringLength(16)]
        public string LanguageCode { get; set; }
        public decimal TaxPercentRate { get; set; }

        [StringLength(128)]
        public string ShoppingCartId { get; set; }

        [StringLength(128)]
        public string PurchaseOrderNumber { get; set; }

        public bool IsAnonymous { get; set; }

        #region Navigation Properties

        public virtual ObservableCollection<TaxDetailEntity> TaxDetails { get; set; } = new NullCollection<TaxDetailEntity>();

        public virtual ObservableCollection<AddressEntity> Addresses { get; set; } = new NullCollection<AddressEntity>();

        public virtual ObservableCollection<PaymentInEntity> InPayments { get; set; } = new NullCollection<PaymentInEntity>();

        public virtual ObservableCollection<LineItemEntity> Items { get; set; } = new NullCollection<LineItemEntity>();

        public virtual ObservableCollection<ShipmentEntity> Shipments { get; set; } = new NullCollection<ShipmentEntity>();

        public virtual ObservableCollection<DiscountEntity> Discounts { get; set; } = new NullCollection<DiscountEntity>();

        public virtual ObservableCollection<FeeDetailEntity> FeeDetails { get; set; } = new NullCollection<FeeDetailEntity>();

        public virtual ObservableCollection<OrderDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; set; }
            = new NullCollection<OrderDynamicPropertyObjectValueEntity>();

        #endregion

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public virtual CustomerOrder ToModel(CustomerOrder order)
        {
            return (CustomerOrder)ToModel((OrderOperation)order);
        }

        public override OrderOperation ToModel(OrderOperation operation)
        {
            var order = operation as CustomerOrder;
            if (order == null)
            {
                throw new ArgumentException(@"operation argument must be of type CustomerOrder", nameof(operation));
            }

            order.RowVersion = RowVersion;
            order.ShoppingCartId = ShoppingCartId;
            order.CustomerId = CustomerId;
            order.CustomerName = CustomerName;
            order.StoreId = StoreId;
            order.StoreName = StoreName;
            order.OrganizationId = OrganizationId;
            order.OrganizationName = OrganizationName;
            order.EmployeeId = EmployeeId;
            order.EmployeeName = EmployeeName;
            order.DiscountAmount = DiscountAmount;
            order.Total = Total;
            order.SubTotal = SubTotal;
            order.SubTotalWithTax = SubTotalWithTax;
            order.ShippingTotal = ShippingTotal;
            order.ShippingTotalWithTax = ShippingTotalWithTax;
            order.PaymentTotal = PaymentTotal;
            order.PaymentTotalWithTax = PaymentTotalWithTax;
            order.Fee = Fee;
            order.FeeWithTax = FeeWithTax;
            order.FeeTotal = FeeTotal;
            order.FeeTotalWithTax = FeeTotalWithTax;
            order.HandlingTotal = HandlingTotal;
            order.HandlingTotalWithTax = HandlingTotalWithTax;
            order.DiscountTotal = DiscountTotal;
            order.DiscountTotalWithTax = DiscountTotalWithTax;
            order.DiscountAmount = DiscountAmount;
            order.TaxTotal = TaxTotal;
            order.IsPrototype = IsPrototype;
            order.SubscriptionNumber = SubscriptionNumber;
            order.SubscriptionId = SubscriptionId;
            order.PurchaseOrderNumber = PurchaseOrderNumber;
            order.LanguageCode = LanguageCode;
            order.TaxPercentRate = TaxPercentRate;
            order.IsAnonymous = IsAnonymous;

            order.Discounts = Discounts.Select(x => x.ToModel(AbstractTypeFactory<Discount>.TryCreateInstance())).ToList();
            order.Items = Items.Select(x => x.ToModel(AbstractTypeFactory<LineItem>.TryCreateInstance())).ToList();
            order.Addresses = Addresses.Select(x => x.ToModel(AbstractTypeFactory<Address>.TryCreateInstance())).ToList();
            order.Shipments = Shipments.Select(x => x.ToModel(AbstractTypeFactory<Shipment>.TryCreateInstance())).OfType<Shipment>().ToList();
            order.InPayments = InPayments.Select(x => x.ToModel(AbstractTypeFactory<PaymentIn>.TryCreateInstance())).OfType<PaymentIn>().ToList();
            order.TaxDetails = TaxDetails.Select(x => x.ToModel(AbstractTypeFactory<TaxDetail>.TryCreateInstance())).ToList();
            order.FeeDetails = FeeDetails.Select(x => x.ToModel(AbstractTypeFactory<FeeDetail>.TryCreateInstance())).ToList();

            order.DynamicProperties = DynamicPropertyObjectValues.GroupBy(g => g.PropertyId).Select(x =>
            {
                var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                property.Id = x.Key;
                property.Name = x.FirstOrDefault()?.PropertyName;
                property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToArray();
                return property;
            }).ToArray();

            //Link shipment item with order lineItem 
            foreach (var shipmentItem in order.Shipments.SelectMany(x => x.Items))
            {
                shipmentItem.LineItem = order.Items.FirstOrDefault(x => x.Id == shipmentItem.LineItemId);
            }

            base.ToModel(order);

            Sum = order.Total;

            return order;
        }

        public virtual CustomerOrderEntity FromModel(CustomerOrder order, PrimaryKeyResolvingMap pkMap)
        {
            return (CustomerOrderEntity)FromModel((OrderOperation)order, pkMap);
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            var order = operation as CustomerOrder;
            if (order == null)
            {
                throw new ArgumentException(@"operation argument must be of type CustomerOrder", nameof(operation));
            }

            base.FromModel(order, pkMap);

            RowVersion = order.RowVersion;
            ShoppingCartId = order.ShoppingCartId;
            CustomerId = order.CustomerId;
            CustomerName = order.CustomerName;
            StoreId = order.StoreId;
            StoreName = order.StoreName;
            OrganizationId = order.OrganizationId;
            OrganizationName = order.OrganizationName;
            EmployeeId = order.EmployeeId;
            EmployeeName = order.EmployeeName;
            DiscountAmount = order.DiscountAmount;
            Total = order.Total;
            SubTotal = order.SubTotal;
            SubTotalWithTax = order.SubTotalWithTax;
            ShippingTotal = order.ShippingTotal;
            ShippingTotalWithTax = order.ShippingTotalWithTax;
            PaymentTotal = order.PaymentTotal;
            PaymentTotalWithTax = order.PaymentTotalWithTax;
            Fee = order.Fee;
            FeeWithTax = order.FeeWithTax;
            FeeTotal = order.FeeTotal;
            FeeTotalWithTax = order.FeeTotalWithTax;
            HandlingTotal = order.HandlingTotal;
            HandlingTotalWithTax = order.HandlingTotalWithTax;
            DiscountTotal = order.DiscountTotal;
            DiscountTotalWithTax = order.DiscountTotalWithTax;
            DiscountAmount = order.DiscountAmount;
            TaxTotal = order.TaxTotal;
            IsPrototype = order.IsPrototype;
            SubscriptionNumber = order.SubscriptionNumber;
            SubscriptionId = order.SubscriptionId;
            PurchaseOrderNumber = order.PurchaseOrderNumber;
            LanguageCode = order.LanguageCode;
            TaxPercentRate = order.TaxPercentRate;
            IsAnonymous = order.IsAnonymous;

            if (order.Addresses != null)
            {
                Addresses = new ObservableCollection<AddressEntity>(order.Addresses.Select(x =>
                    AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(x)));
            }

            if (order.Items != null)
            {
                Items = new ObservableCollection<LineItemEntity>(order.Items.Select(x =>
                    AbstractTypeFactory<LineItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
            }

            if (order.Shipments != null)
            {
                Shipments = new ObservableCollection<ShipmentEntity>(order.Shipments
                    .Select(x => AbstractTypeFactory<ShipmentEntity>.TryCreateInstance().FromModel(x, pkMap))
                    .OfType<ShipmentEntity>());
                //Link shipment item with order lineItem
                foreach (var shipmentItemEntity in Shipments.SelectMany(x => x.Items))
                {
                    // cannot compare using Entity.Equals here since line items can have Id==null
                    shipmentItemEntity.LineItem =
                        Items.FirstOrDefault(x => x.ModelLineItem?.ProductId == shipmentItemEntity.ModelLineItem?.ProductId
                            && x.ModelLineItem?.Quantity == shipmentItemEntity.ModelLineItem?.Quantity);
                }
            }

            if (order.InPayments != null)
            {
                InPayments = new ObservableCollection<PaymentInEntity>(order.InPayments
                    .Select(x => AbstractTypeFactory<PaymentInEntity>.TryCreateInstance().FromModel(x, pkMap))
                    .OfType<PaymentInEntity>());
            }

            if (order.Discounts != null)
            {
                Discounts = new ObservableCollection<DiscountEntity>();
                foreach (var discount in order.Discounts)
                {
                    var discountEntity = AbstractTypeFactory<DiscountEntity>.TryCreateInstance().FromModel(discount);
                    Discounts.Add(discountEntity);
                    pkMap.AddPair(discount, discountEntity);
                }
            }

            if (order.TaxDetails != null)
            {
                TaxDetails = new ObservableCollection<TaxDetailEntity>(order.TaxDetails.Select(x =>
                    AbstractTypeFactory<TaxDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            if (order.FeeDetails != null)
            {
                FeeDetails = new ObservableCollection<FeeDetailEntity>(order.FeeDetails.Select(x =>
                    AbstractTypeFactory<FeeDetailEntity>.TryCreateInstance().FromModel(x)));
            }

            if (order.DynamicProperties != null)
            {
                DynamicPropertyObjectValues = new ObservableCollection<OrderDynamicPropertyObjectValueEntity>(order.DynamicProperties.SelectMany(p => p.Values
                    .Select(v => AbstractTypeFactory<OrderDynamicPropertyObjectValueEntity>.TryCreateInstance().FromModel(v, order, p))).OfType<OrderDynamicPropertyObjectValueEntity>());
            }

            Sum = order.Total;

            return this;
        }

        public virtual void Patch(CustomerOrderEntity target)
        {
            Patch((OperationEntity)target);
        }

        public override void Patch(OperationEntity target)
        {
            var operation = target as CustomerOrderEntity;
            if (operation == null)
            {
                throw new ArgumentException(@"target argument must be of type CustomerOrderEntity",
                    nameof(target));
            }

            operation.RowVersion = RowVersion;
            operation.ShoppingCartId = ShoppingCartId;
            operation.CustomerId = CustomerId;
            operation.CustomerName = CustomerName;
            operation.StoreId = StoreId;
            operation.StoreName = StoreName;
            operation.OrganizationId = OrganizationId;
            operation.OrganizationName = OrganizationName;
            operation.EmployeeId = EmployeeId;
            operation.EmployeeName = EmployeeName;
            operation.IsPrototype = IsPrototype;
            operation.SubscriptionNumber = SubscriptionNumber;
            operation.SubscriptionId = SubscriptionId;
            operation.PurchaseOrderNumber = PurchaseOrderNumber;
            operation.LanguageCode = LanguageCode;
            operation.IsAnonymous = IsAnonymous;

            // Checks whether calculation of sum is needed to pass the result to the property of base class before calling of base.Patch
            var needPatchPrices = !(GetNonCalculatablePrices().All(x => x == 0m) &&
                                    operation.GetNonCalculatablePrices().Any(x => x != 0m));

            if (needPatchPrices)
            {
                operation.Total = Total;
                operation.SubTotal = SubTotal;
                operation.SubTotalWithTax = SubTotalWithTax;
                operation.ShippingTotal = ShippingTotal;
                operation.ShippingTotalWithTax = ShippingTotalWithTax;
                operation.PaymentTotal = PaymentTotal;
                operation.PaymentTotalWithTax = PaymentTotalWithTax;
                operation.HandlingTotal = HandlingTotal;
                operation.HandlingTotalWithTax = HandlingTotalWithTax;
                operation.DiscountTotal = DiscountTotal;
                operation.DiscountTotalWithTax = DiscountTotalWithTax;
                operation.DiscountAmount = DiscountAmount;
                operation.TaxTotal = TaxTotal;
                operation.TaxPercentRate = TaxPercentRate;
                operation.Fee = Fee;
                operation.FeeWithTax = FeeWithTax;
                operation.FeeTotal = FeeTotal;
                operation.FeeTotalWithTax = FeeTotalWithTax;
                operation.HandlingTotal = HandlingTotal;
                operation.HandlingTotalWithTax = HandlingTotalWithTax;
            }

            if (!Addresses.IsNullCollection())
            {
                Addresses.Patch(operation.Addresses, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!Shipments.IsNullCollection())
            {
                foreach (var shipment in Shipments.Where(x => !x.Items.IsNullCollection()))
                {
                    //Need to remove all items from the shipment with references to non-existing line items.
                    //Left join shipment.Items with cart.Items to detect shipment items are referenced to no longer exist line items
                    var toRemoveItems = shipment.Items.GroupJoin(Items,
                            shipmentItem => shipmentItem.LineItemId ?? shipmentItem.LineItem?.Id,
                            lineItem => lineItem.Id,
                            (shipmentItem, lineItem) => new
                            {
                                ShipmentItem = shipmentItem,
                                LineItem = lineItem.SingleOrDefault()
                            })
                        .Where(x => x.LineItem == null)
                        .Select(x => x.ShipmentItem)
                        .ToArray();
                    foreach (var toRemoveItem in toRemoveItems)
                    {
                        shipment.Items.Remove(toRemoveItem);
                    }

                    //Trying to set appropriator lineItem  from EF dynamic proxy lineItem to avoid EF exception (if shipmentItem.LineItem is new object with Id for already exist LineItem)
                    foreach (var shipmentItem in shipment.Items)
                    {
                        if (shipmentItem.LineItem != null)
                        {
                            shipmentItem.LineItem = operation.Items.FirstOrDefault(x => x == shipmentItem.LineItem) ??
                                                    shipmentItem.LineItem;
                        }
                    }
                }

                Shipments.Patch(operation.Shipments,
                    (sourceShipment, targetShipment) => sourceShipment.Patch(targetShipment));
            }

            if (!Items.IsNullCollection())
            {
                Items.Patch(operation.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!InPayments.IsNullCollection())
            {
                InPayments.Patch(operation.InPayments,
                    (sourcePayment, targetPayment) => sourcePayment.Patch(targetPayment));
            }

            if (!Discounts.IsNullCollection())
            {
                var discountComparer = AnonymousComparer.Create((DiscountEntity x) => $"{x.PromotionId}-{x.CouponCode}");
                Discounts.Patch(operation.Discounts, discountComparer,
                    (sourceDiscount, targetDiscount) => sourceDiscount.Patch(targetDiscount));
            }

            if (!TaxDetails.IsNullCollection())
            {
                var taxDetailComparer = AnonymousComparer.Create((TaxDetailEntity x) => x.Name);
                TaxDetails.Patch(operation.TaxDetails, taxDetailComparer,
                    (sourceTaxDetail, targetTaxDetail) => sourceTaxDetail.Patch(targetTaxDetail));
            }

            if (!FeeDetails.IsNullCollection())
            {
                var feeDetailComparer = AnonymousComparer.Create((FeeDetailEntity x) => x.FeeId);
                FeeDetails.Patch(operation.FeeDetails, feeDetailComparer,
                    (sourceFeeDetail, targetFeeDetail) => sourceFeeDetail.Patch(targetFeeDetail));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(operation.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }

            base.Patch(operation);
        }

        public virtual void ResetPrices()
        {
            TaxPercentRate = 0m;
            ShippingTotalWithTax = 0m;
            PaymentTotalWithTax = 0m;
            DiscountAmount = 0m;
            Total = 0m;
            SubTotal = 0m;
            SubTotalWithTax = 0m;
            ShippingTotal = 0m;
            PaymentTotal = 0m;
            Fee = 0m;
            FeeWithTax = 0m;
            FeeTotal = 0;
            FeeTotalWithTax = 0m;
            HandlingTotal = 0m;
            HandlingTotalWithTax = 0m;
            DiscountTotal = 0m;
            DiscountTotalWithTax = 0m;
            TaxTotal = 0m;
            Sum = 0m;

            foreach (var payment in InPayments)
            {
                payment.ResetPrices();
            }

            foreach (var shipment in Shipments)
            {
                shipment.ResetPrices();
            }

            foreach (var item in Items)
            {
                item.ResetPrices();
            }
        }

        public virtual IEnumerable<decimal> GetNonCalculatablePrices()
        {
            yield return TaxPercentRate;
            yield return ShippingTotalWithTax;
            yield return PaymentTotalWithTax;
            yield return DiscountAmount;
            yield return Fee;
        }
    }
}
