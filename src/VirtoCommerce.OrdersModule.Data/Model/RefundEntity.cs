using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class RefundEntity : OperationEntity, IDataEntity<RefundEntity, Refund>
    {
        [Column(TypeName = "Money")]
        public decimal Amount { get; set; }

        public string ReasonCode { get; set; }
        public string ReasonMessage { get; set; }

        public string RejectReasonMessage { get; set; }

        [StringLength(256)]
        public string VendorId { get; set; }

        public string CustomerOrderId { get; set; }
        public virtual CustomerOrderEntity CustomerOrder { get; set; }

        #region Navigation Properties

        public string PaymentId { get; set; }
        public virtual PaymentInEntity Payment { get; set; }

        public virtual ObservableCollection<RefundItemEntity> Items { get; set; } = new NullCollection<RefundItemEntity>();
        public virtual ObservableCollection<OrderDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; set; }
            = new NullCollection<OrderDynamicPropertyObjectValueEntity>();

        #endregion

        public virtual Refund ToModel(Refund model)
        {
            return (Refund)ToModel((OrderOperation)model);
        }

        public override OrderOperation ToModel(OrderOperation operation)
        {
            if (operation is not Refund refund)
            {
                throw new ArgumentException($"{nameof(operation)} argument must be of type {nameof(Refund)}", nameof(operation));
            }

            base.ToModel(refund);

            refund.CustomerOrderId = CustomerOrderId;
            refund.Amount = Amount;
            refund.VendorId = VendorId;
            refund.ReasonMessage = ReasonMessage;
            refund.PaymentId = PaymentId;

            refund.RefundStatus = EnumUtility.SafeParse(Status, RefundStatus.Pending);
            refund.ReasonCode = EnumUtility.SafeParse(ReasonCode, RefundReasonCode.Other);

            refund.Items = Items.Select(x => x.ToModel(AbstractTypeFactory<RefundItem>.TryCreateInstance())).ToList();

            refund.DynamicProperties = DynamicPropertyObjectValues.GroupBy(g => g.PropertyId).Select(x =>
            {
                var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                property.Id = x.Key;
                property.Name = x.FirstOrDefault()?.PropertyName;
                property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToArray();
                return property;
            }).ToArray();

            return refund;
        }

        public virtual RefundEntity FromModel(Refund model, PrimaryKeyResolvingMap pkMap)
        {
            return (RefundEntity)FromModel((OrderOperation)model, pkMap);
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            if (operation is not Refund refund)
            {
                throw new ArgumentException($"{nameof(operation)} argument must be of type {nameof(Refund)}", nameof(operation));
            }

            base.FromModel(refund, pkMap);

            CustomerOrderId = refund.CustomerOrderId;
            Amount = refund.Amount;
            VendorId = refund.VendorId;

            if (refund.Status.IsNullOrEmpty())
            {
                Status = refund.RefundStatus.ToString();
            }

            ReasonCode = refund.ReasonCode.ToString();
            ReasonMessage = refund.ReasonMessage;

            if (refund.Items != null)
            {
                Items = new ObservableCollection<RefundItemEntity>(refund.Items.Select(x => AbstractTypeFactory<RefundItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
                foreach (var refundItem in Items)
                {
                    refundItem.RefundId = Id;
                }
            }

            if (refund.DynamicProperties != null)
            {
                DynamicPropertyObjectValues = new ObservableCollection<OrderDynamicPropertyObjectValueEntity>(refund.DynamicProperties.SelectMany(p => p.Values
                    .Select(v => AbstractTypeFactory<OrderDynamicPropertyObjectValueEntity>.TryCreateInstance().FromModel(v, refund, p))).OfType<OrderDynamicPropertyObjectValueEntity>());
            }

            return this;
        }

        public virtual void Patch(RefundEntity target)
        {
            Patch((OperationEntity)target);
        }

        public override void Patch(OperationEntity target)
        {
            if (target is not RefundEntity refund)
            {
                throw new ArgumentException($"{nameof(target)} argument must be of type {nameof(RefundEntity)}", nameof(target));
            }

            base.Patch(refund);

            refund.VendorId = VendorId;
            refund.Amount = Amount;
            refund.CustomerOrderId = CustomerOrderId;
            refund.ReasonCode = ReasonCode;
            refund.ReasonMessage = ReasonMessage;

            if (!Items.IsNullCollection())
            {
                Items.Patch(refund.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(refund.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }
        }
    }
}
