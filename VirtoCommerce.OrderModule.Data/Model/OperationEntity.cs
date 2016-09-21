using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
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

        public virtual OrderOperation ToModel(OrderOperation orderOperation)
        {
            if (orderOperation == null)
                throw new ArgumentNullException("orderOperation");

            orderOperation.InjectFrom(this);

            orderOperation.ChildrenOperations = orderOperation.GetFlatObjectsListWithInterface<IOperation>().Except(new[] { orderOperation }).ToList();
            return orderOperation;
        }

        public virtual OperationEntity FromModel(OrderOperation orderOperation, PrimaryKeyResolvingMap pkMap)
        {
            if (orderOperation == null)
                throw new ArgumentNullException("orderOperation");

            pkMap.AddPair(orderOperation, this);

            this.InjectFrom(orderOperation);

            return this;
        }

        public virtual void Patch(OperationEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.Comment = this.Comment;
            target.Currency = this.Currency;
            target.Number = this.Number;
            target.Status = this.Status;
            target.IsCancelled = this.IsCancelled;
            target.CancelledDate = this.CancelledDate;
            target.CancelReason = this.CancelReason;       
            target.IsApproved = this.IsApproved;
            target.Sum = this.Sum;
            
        }
    }
}
