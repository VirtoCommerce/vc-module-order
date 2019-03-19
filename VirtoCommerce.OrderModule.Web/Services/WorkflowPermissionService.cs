using VirtoCommerce.OrderModule.Core.Services;
using VirtoCommerce.OrderModule.Web.Security;

namespace VirtoCommerce.OrderModule.Web.Services
{
    public class WorkflowPermissionService : IWorkflowPermissionService
    {
        public string ManagerPermission => WorkflowPredefinedPermissions.Manager;
    }
}
