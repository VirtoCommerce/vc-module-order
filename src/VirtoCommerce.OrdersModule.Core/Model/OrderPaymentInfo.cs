using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class OrderPaymentInfo
    {
        public CustomerOrder CustomerOrder { get; set; }

        public PaymentIn Payment { get; set; }

        public Store Store { get; set; }
    }
}
