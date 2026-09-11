using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Services;

public class PaymentDataProtectionService(
    ICustomerOrderService orderService,
    ICustomerOrderDataProtectionService orderDataProtectionService,
    IPaymentService crudService,
    IPaymentSearchService searchService)
    : OrderOperationDataProtectionService<PaymentIn, PaymentSearchCriteria, PaymentSearchResult>(
        orderService, orderDataProtectionService, crudService, searchService),
      IPaymentDataProtectionService
{
    protected override string GetOrderId(PaymentIn operation) => operation.OrderId;
}
