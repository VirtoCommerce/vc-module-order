using VirtoCommerce.Domain.Order.Model;

namespace VirtoCommerce.OrderModule.Core.Models
{
    /// <summary>
    /// <see cref="CustomerOrder"/>
    /// </summary>
    public class CustomerOrderWorkflow : CustomerOrder
    {
        public CustomerOrderWorkflow()
        {
            OperationType = "CustomerOrder";
        }
        public string WorkflowId { get; set; }

        public string WorkflowStatus { get; set; }
    }
}
