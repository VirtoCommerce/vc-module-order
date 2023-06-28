using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IPaymentSearchService : ISearchService<PaymentSearchCriteria, PaymentSearchResult, PaymentIn>
    {
    }
}
