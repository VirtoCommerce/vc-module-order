using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public class FeeDetailEntity : Entity
    {
        [StringLength(128)]
        public string FeeId { get; set; }

        [StringLength(128)]
        public string Currency { get; set; }

        [Column(TypeName = "Money")]
        public decimal Amount { get; set; }
        public string Description { get; set; }

        #region Navigation Properties

        public string CustomerOrderId { get; set; }
        public virtual CustomerOrderEntity CustomerOrder { get; set; }

        public string ShipmentId { get; set; }
        public virtual ShipmentEntity Shipment { get; set; }

        public string LineItemId { get; set; }
        public virtual LineItemEntity LineItem { get; set; }

        public string PaymentInId { get; set; }
        public virtual PaymentInEntity PaymentIn { get; set; }

        #endregion

        public virtual FeeDetail ToModel(FeeDetail feeDetail)
        {
            if (feeDetail == null)
            {
                throw new ArgumentNullException(nameof(feeDetail));
            }

            feeDetail.Id = Id;
            feeDetail.FeeId = FeeId;
            feeDetail.Currency = Currency;
            feeDetail.Amount = Amount;
            feeDetail.Description = Description;

            return feeDetail;
        }

        public virtual FeeDetailEntity FromModel(FeeDetail feeDetail)
        {
            if (feeDetail == null)
            {
                throw new ArgumentNullException(nameof(feeDetail));
            }

            Id = feeDetail.Id;
            FeeId = feeDetail.FeeId;
            Currency = feeDetail.Currency;
            Amount = feeDetail.Amount;
            Description = feeDetail.Description;

            return this;
        }

        public virtual void Patch(FeeDetailEntity target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.Currency = Currency;
            target.Amount = Amount;
            target.Description = Description;
        }
    }
}
