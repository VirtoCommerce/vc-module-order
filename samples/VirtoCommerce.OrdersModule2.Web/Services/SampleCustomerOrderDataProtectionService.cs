using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.OrdersModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule2.Web.Services;

public sealed class SampleCustomerOrderDataProtectionService(
    ICustomerOrderService crudService,
    ICustomerOrderSearchService searchService,
    IUserNameResolver userNameResolver,
    SignInManager<ApplicationUser> signInManager,
    IStoreService storeService,
    IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
    : CustomerOrderDataProtectionService(crudService, searchService, userNameResolver, signInManager)
{
    private readonly MvcNewtonsoftJsonOptions _jsonOptions = jsonOptions.Value;

    protected override async Task<bool> CanReadPrices(ClaimsPrincipal user, CustomerOrder order)
    {
        var canReadPrices = await base.CanReadPrices(user, order);

        if (!canReadPrices)
        {
            var store = await storeService.GetByIdAsync(order.StoreId);
            canReadPrices = store != null && CanReadPricesForStore(user, store);
        }

        return canReadPrices;
    }

    private bool CanReadPricesForStore(ClaimsPrincipal user, Store store)
    {
        var isDirectDistributor = store.Name.ContainsIgnoreCase("Direct");

        var permissionName = isDirectDistributor
            ? ModuleConstants.Permissions.ReadPricesDirect
            : ModuleConstants.Permissions.ReadPricesIndirect;

        var permission = user.FindPermission(permissionName, _jsonOptions.SerializerSettings);

        return permission != null;
    }
}
