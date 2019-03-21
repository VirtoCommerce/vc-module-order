
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.OrderModule.Core.Models;

namespace VirtoCommerce.OrderModule.Core.Services
{
    public interface IWorkflowService
    {
        Workflow ImportOrUpdateWorkflow(Workflow workflowModel);

        Workflow GetByOrganizationId(string organizationId);

        GenericSearchResult<Workflow> Search(WorkflowSearchCriteria searchWorkflowCriteria);
    }
}
