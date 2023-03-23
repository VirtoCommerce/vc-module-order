using VirtoCommerce.PaymentModule.Core.Model;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public static class PaymentErrorDescriber
    {
        public static string InvalidStatus(PaymentStatus status)
        {
            return $"Unable to process due to invalid payment status: {status}";
        }

        public static string OrderNotFound()
        {
            return $"Can't find customer order";
        }

        public static string PaymentNotFound()
        {
            return $"Can't find payment in order";
        }

        public static string PaymentMethodNotFound(string gatewayCode)
        {
            return $"Can't find payment method with code: '{gatewayCode ?? "null"}'";
        }

        public static string StoreNotFound()
        {
            return $"Can't find a store";
        }

        public static string NotCapturable()
        {
            return $"Payment method does not support payment capture";
        }

        public static string NotRefundable()
        {
            return $"Payment method does not support refund";
        }

        public static string PaymentMethodError()
        {
            return $"Internal payment method error";
        }
    }
}
