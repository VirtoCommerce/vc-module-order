using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.OrderModule.Data.Utilities;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Data.Model
{
    public abstract class OperationEntity : AuditableEntity
    {
        [Required]
        [StringLength(64)]
        public string Number { get; set; }
        public bool IsApproved { get; set; }
        [StringLength(64)]
        public string Status { get; set; }
        [StringLength(2048)]
        public string Comment { get; set; }
        [Required]
        [StringLength(3)]
        public string Currency { get; set; }
        [Column(TypeName = "Money")]
        public decimal Sum { get; set; }

        public bool IsCancelled { get; set; }
        public DateTime? CancelledDate { get; set; }
        [StringLength(2048)]
        public string CancelReason { get; set; }

        [NotMapped]
        public bool NeedPatchSum { get; set; } = true;

        public virtual OrderOperation ToModel(OrderOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            operation.InjectFrom(this);

            operation.ChildrenOperations = OperationUtilities.GetAllChildOperations(operation);
            return operation;
        }

        public virtual OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            pkMap.AddPair(operation, this);

            this.InjectFrom(operation);

            return this;
        }

        public virtual void Patch(OperationEntity operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            operation.Comment = Comment;
            operation.Currency = Currency;
            operation.Number = Number;
            operation.Status = Status;
            operation.IsCancelled = IsCancelled;
            operation.CancelledDate = CancelledDate;
            operation.CancelReason = CancelReason;
            operation.IsApproved = IsApproved;

            if (NeedPatchSum)
            {
                operation.Sum = Sum;
            }
        }
    }
}
