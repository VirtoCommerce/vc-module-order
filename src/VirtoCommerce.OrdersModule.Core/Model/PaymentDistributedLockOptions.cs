using System;

namespace VirtoCommerce.OrdersModule.Core.Model
{
    public class PaymentDistributedLockOptions
    {
        /// <summary>
        /// The maximum duration the resource will be locked. Default: 30 seconds.
        /// </summary>
        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(30);
        /// <summary>
        /// The duration to attempt acquiring the locked resource. Default: 30 seconds.
        /// </summary>
        public TimeSpan TryLockTimeout { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// The interval between lock acquisition retries. Default: 1 second.
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}
