using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IShipmentService
    {
        Task SaveChangesAsync(Shipment[] shipments);
    }
}
