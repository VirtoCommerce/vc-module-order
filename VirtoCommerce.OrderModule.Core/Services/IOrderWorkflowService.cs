using System.Collections.Generic;
using VirtoCommerce.OrderModule.Core.Models;

namespace VirtoCommerce.OrderModule.Core.Services
{
    public interface IOrderWorkflowService
    {
        OrderWorkflow GetOrderWorkflow(string orderId);
        OrderWorkflow AddOrderWorkflow(OrderWorkflow model);
    }
}
