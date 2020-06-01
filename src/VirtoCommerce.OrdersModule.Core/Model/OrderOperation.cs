using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
        [JsonIgnore]
        public IEnumerable<IOperation> ChildrenOperations { get; set; }

        #region ISupportCancelation Members

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
