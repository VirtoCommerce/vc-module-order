using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public abstract class OrderOperation : AuditableEntity, IOperation, ISupportCancellation, IHasDynamicProperties, IHasChangesHistory, ICloneable
    {
        protected OrderOperation()
        {
            OperationType = GetType().Name;
        }

        public string OperationType { get; set; }

        public string ParentOperationId { get; set; }

        [Auditable]
        public string Number { get; set; }

        [Auditable]
        public bool IsApproved { get; set; }

        [Auditable]
        public string Status { get; set; }

        [Auditable]
        public string Comment { get; set; }

        [Auditable]
        public string Currency { get; set; }

        public decimal Sum { get; set; }

        [Auditable]
        public string OuterId { get; set; }

        [SwaggerIgnore]
        public IEnumerable<IOperation> ChildrenOperations { get; set; }

        #region ISupportCancelation Members

        /// <summary>
        /// For system use to handle canellation flow
        /// </summary>
        public CancelledState CancelledState { get; set; }

        /// <summary>
        /// Used by payment provides to indicate that cancellation operation has completed
        /// </summary>
        public bool IsCancelled { get; set; }

        public DateTime? CancelledDate { get; set; }

        public string CancelReason { get; set; }

        #endregion

        #region IHasDynamicProperties Members

        public virtual string ObjectType { get; set; }

        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; }

        #endregion

        #region IHasChangesHistory

        public ICollection<OperationLog> OperationsLog { get; set; }

        #endregion

        #region ICloneable members

        public virtual object Clone()
        {
            var result = MemberwiseClone() as OrderOperation;

            result.DynamicProperties = DynamicProperties?.Select(x => x.Clone()).OfType<DynamicObjectProperty>().ToList();
            result.OperationsLog = OperationsLog?.Select(x => x.Clone()).OfType<OperationLog>().ToList();

            return result;
        }

        #endregion
    }
}
