using System.Collections.Specialized;

namespace VirtoCommerce.OrdersModule.Core.Model;

public class PaymentParameters
{
    public string OrderId { get; set; }
    public string PaymentMethodCode { get; set; }
    public NameValueCollection Parameters { get; set; } = new();
}
