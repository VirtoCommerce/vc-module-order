using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class CaptureEntity : OperationEntity, IDataEntity<CaptureEntity, Capture>
    {
        [Column(TypeName = "Money")]
        public decimal Amount { get; set; }

        [StringLength(256)]
        public string VendorId { get; set; }

        [Required]
        [StringLength(128)]
        public string TransactionId { get; set; }

        public string CustomerOrderId { get; set; }
        public virtual CustomerOrderEntity CustomerOrder { get; set; }

        public bool CloseTransaction { get; set; }

        #region Navigation Properties

        public string PaymentId { get; set; }
        public virtual PaymentInEntity Payment { get; set; }

        public virtual ObservableCollection<CaptureItemEntity> Items { get; set; } = new NullCollection<CaptureItemEntity>();
        public virtual ObservableCollection<OrderDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues { get; set; }
            = new NullCollection<OrderDynamicPropertyObjectValueEntity>();

        #endregion

        public virtual Capture ToModel(Capture model)
        {
            return (Capture)ToModel((OrderOperation)model);
        }

        public override OrderOperation ToModel(OrderOperation operation)
        {
            if (operation is not Capture capture)
            {
                throw new ArgumentException($"{nameof(operation)} argument must be of type {nameof(Capture)}", nameof(operation));
            }

            base.ToModel(capture);

            capture.CustomerOrderId = CustomerOrderId;
            capture.Amount = Amount;
            capture.VendorId = VendorId;
            capture.TransactionId = TransactionId;
            capture.CloseTransaction = CloseTransaction;
            capture.PaymentId = PaymentId;
            capture.Status = Status;

            capture.Items = Items.Select(x => x.ToModel(AbstractTypeFactory<CaptureItem>.TryCreateInstance())).ToList();

            capture.DynamicProperties = DynamicPropertyObjectValues.GroupBy(g => g.PropertyId).Select(x =>
            {
                var property = AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance();
                property.Id = x.Key;
                property.Name = x.FirstOrDefault()?.PropertyName;
                property.Values = x.Select(v => v.ToModel(AbstractTypeFactory<DynamicPropertyObjectValue>.TryCreateInstance())).ToArray();
                return property;
            }).ToArray();

            return capture;
        }

        public virtual CaptureEntity FromModel(Capture model, PrimaryKeyResolvingMap pkMap)
        {
            return (CaptureEntity)FromModel((OrderOperation)model, pkMap);
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            if (operation is not Capture capture)
            {
                throw new ArgumentException($"{nameof(operation)} argument must be of type {nameof(Capture)}", nameof(operation));
            }

            base.FromModel(capture, pkMap);

            CustomerOrderId = capture.CustomerOrderId;
            Amount = capture.Amount;
            VendorId = capture.VendorId;
            TransactionId = capture.TransactionId;
            Status = capture.Status;
            CloseTransaction = capture.CloseTransaction;

            if (capture.Items != null)
            {
                Items = new ObservableCollection<CaptureItemEntity>(capture.Items.Select(x => AbstractTypeFactory<CaptureItemEntity>.TryCreateInstance().FromModel(x, pkMap)));
                foreach (var captureItem in Items)
                {
                    captureItem.CaptureId = Id;
                }
            }

            if (capture.DynamicProperties != null)
            {
                DynamicPropertyObjectValues = new ObservableCollection<OrderDynamicPropertyObjectValueEntity>(capture.DynamicProperties.SelectMany(p => p.Values
                    .Select(v => AbstractTypeFactory<OrderDynamicPropertyObjectValueEntity>.TryCreateInstance().FromModel(v, capture, p))).OfType<OrderDynamicPropertyObjectValueEntity>());
            }

            return this;
        }

        public virtual void Patch(CaptureEntity target)
        {
            Patch((OperationEntity)target);
        }

        public override void Patch(OperationEntity target)
        {
            if (target is not CaptureEntity capture)
            {
                throw new ArgumentException($"{nameof(target)} argument must be of type {nameof(CaptureEntity)}", nameof(target));
            }

            base.Patch(capture);

            capture.VendorId = VendorId;
            capture.TransactionId = TransactionId;
            capture.Amount = Amount;
            capture.CustomerOrderId = CustomerOrderId;
            capture.CloseTransaction = CloseTransaction;

            if (!Items.IsNullCollection())
            {
                Items.Patch(capture.Items, (sourceItem, targetItem) => sourceItem.Patch(targetItem));
            }

            if (!DynamicPropertyObjectValues.IsNullCollection())
            {
                DynamicPropertyObjectValues.Patch(capture.DynamicPropertyObjectValues, (sourceDynamicPropertyObjectValues, targetDynamicPropertyObjectValues) => sourceDynamicPropertyObjectValues.Patch(targetDynamicPropertyObjectValues));
            }
        }
    }
}
