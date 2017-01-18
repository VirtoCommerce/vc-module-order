using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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

            orderOperation.ChildrenOperations = GetAllChildOperations(orderOperation);
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

        private static IEnumerable<IOperation> GetAllChildOperations(IOperation operation)
        {
            var retVal = new List<IOperation>();
            var objectType = operation.GetType();

            var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var childOperations = properties.Where(x => x.PropertyType.GetInterface(typeof(IOperation).Name) != null)
                                    .Select(x => (IOperation)x.GetValue(operation)).Where(x=> x != null).ToList();

            foreach (var childOperation in childOperations.OfType<IOperation>())
            {
                retVal.Add(childOperation);
            }

            //Handle collection and arrays
            var collections = properties.Where(p => p.GetIndexParameters().Length == 0)
                                        .Select(x => x.GetValue(operation, null))
                                        .Where(x => x is IEnumerable && !(x is String))
                                        .Cast<IEnumerable>();

            foreach (var collection in collections)
            {
                foreach (var childOperation in collection.OfType<IOperation>())
                {
                    retVal.Add(childOperation);                   
                }
            }
            return retVal;
        }
    }
}
