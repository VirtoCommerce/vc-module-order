using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IShipmentSearchService : ISearchService<ShipmentSearchCriteria, ShipmentSearchResult, Shipment>
    {
    }
}
