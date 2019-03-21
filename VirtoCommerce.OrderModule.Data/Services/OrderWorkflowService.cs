using System.Linq;
using VirtoCommerce.OrderModule.Core.Models;
using VirtoCommerce.OrderModule.Core.Services;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrderModule.Data.Services
{
    public class OrderWorkflowService : ServiceBase, IOrderWorkflowService
    {
        private IOrderRepository _repositoryFactory;

        public OrderWorkflowService(IOrderRepository repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public OrderWorkflow AddOrderWorkflow(OrderWorkflow orderWorkflowModel)
        {
            var orderWorkflow = new OrderWorkflowEntity
            {
                OrderId = orderWorkflowModel.OrderId,
                WorkflowId = orderWorkflowModel.WorkflowId
            };
            using (var changeTracker = GetChangeTracker(_repositoryFactory))
            {
                changeTracker.Attach(orderWorkflow);
                _repositoryFactory.Add(orderWorkflow);
                CommitChanges(_repositoryFactory);
            }
            return orderWorkflow.ToModel(AbstractTypeFactory<OrderWorkflow>.TryCreateInstance());
        }

        public OrderWorkflow GetOrderWorkflow(string orderId)
        {
            _repositoryFactory.DisableChangesTracking();
            return _repositoryFactory.OrderWorkflows.FirstOrDefault(x => x.OrderId == orderId)?.ToModel(AbstractTypeFactory<OrderWorkflow>.TryCreateInstance());
        }
    }
}
