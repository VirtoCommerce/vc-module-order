using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public class PaymentGatewayTransactionEntity : AuditableEntity
    {
        [Column(TypeName = "Money")]
        public decimal Amount { get; set; }
        [StringLength(3)]
        public string Currency { get; set; }
          
        public bool IsProcessed { get; set; }      
        public DateTime? ProcessedDate { get; set; }
        [StringLength(2048)]
        public string ProcessError { get; set; }
        public int ProcessAttemptCount { get; set; }

        public string RequestData { get; set; }

        public string ResponseData { get; set; }

        [StringLength(64)]
        public string ResponseCode { get; set; }

        /// <summary>
        /// Gateway IP address
        /// </summary>
        [StringLength(128)]
        public string GatewayIpAddress { get; set; }

        [StringLength(64)]
        public string Type { get; set; }

        [StringLength(64)]
        public string Status { get; set; }

        [StringLength(2048)]
        public string Note { get; set; }

        public virtual PaymentInEntity PaymentIn { get; set; }
        public string PaymentInId { get; set; }

        public virtual PaymentGatewayTransaction ToModel(PaymentGatewayTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            transaction.Id = this.Id;
            transaction.Amount = this.Amount;
            transaction.CreatedBy = this.CreatedBy;
            transaction.CreatedDate = this.CreatedDate;
            transaction.CurrencyCode = this.Currency;
            transaction.GatewayIpAddress = this.GatewayIpAddress;
            transaction.IsProcessed = this.IsProcessed;
            transaction.ModifiedBy = this.ModifiedBy;
            transaction.ModifiedDate = this.ModifiedDate;
            transaction.Note = this.Note;
            transaction.ProcessAttemptCount = this.ProcessAttemptCount;
            transaction.ProcessedDate = this.ProcessedDate;
            transaction.ProcessError = this.ProcessError;
            transaction.RequestData = this.RequestData;
            transaction.ResponseCode = this.ResponseCode;
            transaction.ResponseData = this.ResponseData;
            transaction.Status = this.Status;
            transaction.Type = this.Type;

            return transaction;
        }

        public virtual PaymentGatewayTransactionEntity FromModel(PaymentGatewayTransaction transaction, PrimaryKeyResolvingMap pkMap)
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction");

            pkMap.AddPair(transaction, this);

            this.Id = transaction.Id;
            this.Amount = transaction.Amount;
            this.CreatedBy = transaction.CreatedBy;
            this.CreatedDate = transaction.CreatedDate;
            this.Currency = transaction.CurrencyCode;
            this.GatewayIpAddress = transaction.GatewayIpAddress;
            this.IsProcessed = transaction.IsProcessed;
            this.ModifiedBy = transaction.ModifiedBy;
            this.ModifiedDate = transaction.ModifiedDate;
            this.Note = transaction.Note;
            this.ProcessAttemptCount = transaction.ProcessAttemptCount;
            this.ProcessedDate = transaction.ProcessedDate;
            this.ProcessError = transaction.ProcessError;
            this.RequestData = transaction.RequestData;
            this.ResponseCode = transaction.ResponseCode;
            this.ResponseData = transaction.ResponseData;
            this.Status = transaction.Status;
            this.Type = transaction.Type;

            return this;
        }

        public virtual void Patch(PaymentGatewayTransactionEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
     
            target.Amount = this.Amount;
        
            target.Currency = this.Currency;
            target.GatewayIpAddress = this.GatewayIpAddress;
            target.IsProcessed = this.IsProcessed;
          
            target.Note = this.Note;
            target.ProcessAttemptCount = this.ProcessAttemptCount;
            target.ProcessedDate = this.ProcessedDate;
            target.ProcessError = this.ProcessError;
            target.RequestData = this.RequestData;
            target.ResponseCode = this.ResponseCode;
            target.ResponseData = this.ResponseData;
            target.Status = this.Status;
            target.Type = this.Type;
        }
    }
}
