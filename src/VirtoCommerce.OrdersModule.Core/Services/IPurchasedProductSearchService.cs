using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Models;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.OrdersModule.Core.Services;

public interface IPurchasedProductSearchService : ISearchService<PurchasedProductSearchCriteria, PurchasedProductSearchResult, PurchasedProduct>;
