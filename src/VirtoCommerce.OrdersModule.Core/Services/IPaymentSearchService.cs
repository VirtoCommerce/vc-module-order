using System;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model.Search;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="SearchService<PaymentIn>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IPaymentSearchService
    {
        [Obsolete(@"Need to remove after inherit IPaymentSearchService from SearchService<PaymentIn>")]
        Task<PaymentSearchResult> SearchPaymentsAsync(PaymentSearchCriteria criteria);
    }
}
