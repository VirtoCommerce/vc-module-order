using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Data.Extensions;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.OrdersModule.Data.Model
{
    public abstract class OperationEntity : AuditableEntity, IHasOuterId, IDataEntity<OperationEntity, OrderOperation>
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

        [StringLength(32)]
        public string CancelledState { get; set; }

        public DateTime? CancelledDate { get; set; }

        [StringLength(2048)]
        public string CancelReason { get; set; }

        [NotMapped]
        public bool NeedPatchSum { get; set; } = true;

        [StringLength(128)]
        public string OuterId { get; set; }

        [StringLength(128)]
        public string ParentOperationId { get; set; }

        public virtual OrderOperation ToModel(OrderOperation operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            operation.Id = Id;
            operation.CreatedDate = CreatedDate;
            operation.CreatedBy = CreatedBy;
            operation.ModifiedDate = ModifiedDate;
            operation.ModifiedBy = ModifiedBy;
            operation.OuterId = OuterId;
            operation.ParentOperationId = ParentOperationId;

            operation.Comment = Comment;
            operation.Currency = Currency;
            operation.Number = Number;
            operation.Status = Status;
            operation.IsCancelled = IsCancelled;
            operation.CancelledState = EnumUtility.SafeParse(CancelledState, Core.Model.CancelledState.Undefined);
            operation.CancelledDate = CancelledDate;
            operation.CancelReason = CancelReason;
            operation.IsApproved = IsApproved;
            operation.Sum = Sum;
            operation.FillAllChildOperations();

            return operation;
        }

        public virtual OperationEntity FromModel(OrderOperation operation, PrimaryKeyResolvingMap pkMap)
        {
            ArgumentNullException.ThrowIfNull(operation);

            pkMap.AddPair(operation, this);

            Id = operation.Id;
            CreatedDate = operation.CreatedDate;
            CreatedBy = operation.CreatedBy;
            ModifiedDate = operation.ModifiedDate;
            ModifiedBy = operation.ModifiedBy;
            OuterId = operation.OuterId;
            ParentOperationId = operation.ParentOperationId;

            Comment = operation.Comment;
            Currency = operation.Currency;
            Number = operation.Number;
            Status = operation.Status;
            IsCancelled = operation.IsCancelled;
            CancelledState = operation.CancelledState.ToString();
            CancelledDate = operation.CancelledDate;
            CancelReason = operation.CancelReason;
            IsApproved = operation.IsApproved;
            Sum = operation.Sum;

            return this;
        }

        public virtual void Patch(OperationEntity target)
        {
            ArgumentNullException.ThrowIfNull(target);

            target.Comment = Comment;
            target.Currency = Currency;
            target.Number = Number;
            target.Status = Status;
            target.IsCancelled = IsCancelled;
            target.CancelledState = CancelledState;
            target.CancelledDate = CancelledDate;
            target.CancelReason = CancelReason;
            target.IsApproved = IsApproved;
            target.OuterId = OuterId;
            target.ParentOperationId = ParentOperationId;

            if (NeedPatchSum)
            {
                target.Sum = Sum;
            }
        }
    }
}
