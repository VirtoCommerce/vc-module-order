using VirtoCommerce.Domain.Commerce.Model.Search;

namespace VirtoCommerce.OrderModule.Core.Models
{
    /// <summary>
    /// Workflow search criteria
    /// </summary>
    public class WorkflowSearchCriteria : SearchCriteriaBase
    {
        public string OrganizationId { get; set; }
        public string WorkflowName { get; set; }
        public bool? Status { get; set; }
    }
}
