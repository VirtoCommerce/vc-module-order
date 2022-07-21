namespace VirtoCommerce.OrdersModule.Core.Services
{
    public interface IMemberOrdersService
    {
        bool IsFirstTimeBuyer(string customerId);
    }
}
