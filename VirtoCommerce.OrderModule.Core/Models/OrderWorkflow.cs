
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrderModule.Core.Models
{
    /// <summary>
    /// </summary>
    public class OrderWorkflow : AuditableEntity
    {
        public string WorkflowId { get; set; }

        public string OrderId { get; set; }
    }
}
