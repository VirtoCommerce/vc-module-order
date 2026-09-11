using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class Capture : OrderOperation
    {
        public override string ObjectType { get; set; } = typeof(Capture).FullName;
        [Auditable]
        public decimal Amount { get; set; }
        [Auditable]
        public string VendorId { get; set; }
        [Auditable]
        public string TransactionId { get; set; }

        public string CustomerOrderId { get; set; }

        public string PaymentId { get; set; }

        public virtual ICollection<CaptureItem> Items { get; set; }
        [Auditable]
        public bool CloseTransaction { get; set; }

        public override void ReduceDetails(string responseGroup)
        {
            base.ReduceDetails(responseGroup);

            // Reduce details according to the response group
            var orderResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CustomerOrderResponseGroup.Full);

            if (!orderResponseGroup.HasFlag(CustomerOrderResponseGroup.WithPrices))
            {
                Amount = 0m;
            }
        }

        public override void RestoreDetails(OrderOperation operation)
        {
            base.RestoreDetails(operation);

            if (operation is not Capture capture)
            {
                return;
            }

            Amount = capture.Amount;
        }
    }
}
