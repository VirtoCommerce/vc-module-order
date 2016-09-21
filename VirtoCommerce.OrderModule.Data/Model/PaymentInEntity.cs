using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
	public class PaymentInEntity : OperationEntity
	{
		public PaymentInEntity()
		{
			Addresses = new NullCollection<AddressEntity>();
		}
		[StringLength(64)]
		public string OrganizationId { get; set; }
		[StringLength(255)]
		public string OrganizationName { get; set; }

		[Required]
		[StringLength(64)]
		public string CustomerId { get; set; }
		[StringLength(255)]
		public string CustomerName { get; set; }

		public DateTime? IncomingDate { get; set; }
		[StringLength(128)]
		public string OuterId { get; set; }
		[StringLength(1024)]
		public string Purpose { get; set; }
		[StringLength(64)]
		public string GatewayCode { get; set; }

		public DateTime? AuthorizedDate { get; set; }
		public DateTime? CapturedDate { get; set; }
		public DateTime? VoidedDate { get; set; }    

        public virtual ObservableCollection<AddressEntity> Addresses { get; set; }

		public string CustomerOrderId { get; set; }
		public virtual CustomerOrderEntity CustomerOrder { get; set; }

		public string ShipmentId { get; set; }
		public virtual ShipmentEntity Shipment { get; set; }

        public override OrderOperation ToModel(OrderOperation operation)
        {        
            var payment = operation as PaymentIn;
            if (payment == null)
                throw new NullReferenceException("payment");

            payment.InjectFrom(this);
            payment.PaymentStatus = EnumUtility.SafeParse<PaymentStatus>(this.Status, PaymentStatus.Custom);

            if (!this.Addresses.IsNullOrEmpty())
            {
                payment.BillingAddress =  this.Addresses.First().ToModel(AbstractTypeFactory<Address>.TryCreateInstance());
            }

            base.ToModel(payment);

            return payment;
        }

        public override OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            var payment = operation as PaymentIn;
            if (payment == null)
                throw new NullReferenceException("payment");

            this.Status = payment.PaymentStatus.ToString();

            if (payment.BillingAddress != null)
            {
                this.Addresses = new ObservableCollection<AddressEntity>(new AddressEntity[] { AbstractTypeFactory<AddressEntity>.TryCreateInstance().FromModel(payment.BillingAddress) });
            }

            base.FromModel(payment, pkMap);

            return this;
        }

        public override void Patch(OperationEntity operation)
        {
            base.Patch(operation);

            var target = operation as PaymentInEntity;
            if (target == null)
                throw new ArgumentNullException("target");

            target.CustomerId = this.CustomerId;
            target.OrganizationId = this.OrganizationId;
            target.GatewayCode = this.GatewayCode;
            target.Purpose = this.Purpose;
            target.OuterId = this.OuterId;
            target.Status = this.Status;
            target.AuthorizedDate = this.AuthorizedDate;
            target.CapturedDate = this.CapturedDate;
            target.VoidedDate = this.VoidedDate;
            target.IsCancelled = this.IsCancelled;
            target.CancelledDate = this.CancelledDate;
            target.CancelReason = this.CancelReason;
            target.Sum = this.Sum;
     
            if (!this.Addresses.IsNullCollection())
            {
                this.Addresses.Patch(target.Addresses, new AddressComparer(), (sourceAddress, targetAddress) => sourceAddress.Patch(targetAddress));
            }
        }
    }
}
