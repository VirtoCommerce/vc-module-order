using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.OrdersModule.Data.Authorization;

public class OrderAuthorizationContext
{
    public AuthorizationHandlerContext HandlerContext { get; set; }
    public OrderAuthorizationRequirement Requirement { get; set; }

    public string UserId { get; set; }
    public string MemberId { get; set; }
    public string EmployeeId { get; set; }

    public Permission UserPermission { get; set; }
    public string[] AllowedStoreIds { get; set; }
    public bool HasResponsibleScope { get; set; }
}
