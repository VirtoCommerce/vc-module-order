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
            var refund = operation as Refund;
            if (refund == null)
            {
                throw new ArgumentException(@"operation argument must be of type Refund", nameof(operation));
            }

            refund.Id = Id;
            refund.CreatedDate = CreatedDate;
            refund.CreatedBy = CreatedBy;
            refund.ModifiedDate = ModifiedDate;
            refund.ModifiedBy = ModifiedBy;
            refund.OuterId = OuterId;

            refund.CustomerOrderId = CustomerOrderId;

            refund.Amount = Amount;
            refund.OuterId = OuterId;
            refund.Status = Status;
            refund.IsCancelled = IsCancelled;
            refund.CancelledDate = CancelledDate;
            refund.CancelReason = CancelReason;
            refund.Sum = Sum;
            refund.VendorId = VendorId;

            refund.Items = Items.Select(x => x.ToModel(AbstractTypeFactory<RefundItem>.TryCreateInstance())).ToList();

            refund.DynamicProperties = DynamicPropertyObjectValues.GroupBy(g => g.PropertyId).Select(x =>
            {
                var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                property.Id = x.Key;
                property.Name = x.FirstOrDefault()?.PropertyName;
                property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToArray();
                return property;
            }).ToArray();

            base.ToModel(refund);

            refund.RefundStatus = EnumUtility.SafeParse(Status, RefundStatus.Pending);

            return refund;
        }

        public virtual RefundEntity FromModel(Refund model, PrimaryKeyResolvingMap pkMap)
        {
            return (RefundEntity)FromModel((OrderOperation)model, pkMap);
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            var refund = operation as Refund;
            if (refund == null)
            {
                throw new ArgumentException(@"operation argument must be of type Refund", nameof(operation));
            }

            base.FromModel(refund, pkMap);

            Id = refund.Id;
            CreatedDate = refund.CreatedDate;
            CreatedBy = refund.CreatedBy;
            ModifiedDate = refund.ModifiedDate;
            ModifiedBy = refund.ModifiedBy;

            CustomerOrderId = refund.CustomerOrderId;

            Amount = refund.Amount;
            OuterId = refund.OuterId;
            Status = refund.Status;
            IsCancelled = refund.IsCancelled;
            CancelledDate = refund.CancelledDate;
            CancelReason = refund.CancelReason;
            Sum = refund.Sum;
            VendorId = refund.VendorId;

            if (refund.Status.IsNullOrEmpty())
            {
                Status = refund.RefundStatus.ToString();
            }

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

        public override void Patch(OperationEntity operation)
        {
            var target = operation as RefundEntity;
            if (target == null)
            {
                throw new ArgumentException(@"target argument must be of type RefunrEntity", nameof(target));
            }

            base.Patch(target);

            target.OuterId = OuterId;
            target.Status = Status;
            target.IsCancelled = IsCancelled;
            target.CancelledDate = CancelledDate;
            target.CancelReason = CancelReason;
            target.VendorId = VendorId;
            target.Sum = Sum;
            target.OuterId = OuterId;
            target.Status = Status;
            target.IsCancelled = IsCancelled;
            target.CancelledDate = CancelledDate;
            target.CancelReason = CancelReason;
            target.VendorId = VendorId;

            if (!Items.IsNullCollection())
            {
                Items.Patch(target.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(target.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }
        }
    }
}
