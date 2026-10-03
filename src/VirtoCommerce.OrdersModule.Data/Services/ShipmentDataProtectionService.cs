using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.OrdersModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Services;

public class ShipmentDataProtectionService(
    ICustomerOrderService orderService,
    ICustomerOrderDataProtectionService orderDataProtectionService,
    IShipmentService crudService,
    IShipmentSearchService searchService)
    : OrderOperationDataProtectionService<Shipment, ShipmentSearchCriteria, ShipmentSearchResult>(
        orderService, orderDataProtectionService, crudService, searchService),
      IShipmentDataProtectionService
{
    protected override string GetOrderId(Shipment operation) => operation.CustomerOrderId;
}
