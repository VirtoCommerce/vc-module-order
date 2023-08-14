using System;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class PaymentDistributedLockOptions
    {
        public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan RetryTime { get; set; } = TimeSpan.FromSeconds(1);
    }
}
