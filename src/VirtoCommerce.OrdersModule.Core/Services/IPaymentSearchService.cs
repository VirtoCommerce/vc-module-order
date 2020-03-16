using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model.Search;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IPaymentSearchService
    {
        Task<PaymentSearchResult> SearchPaymentsAsync(PaymentSearchCriteria criteria);
    }
}
