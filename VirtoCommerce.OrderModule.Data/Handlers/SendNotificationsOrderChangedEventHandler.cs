using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Notifications;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrderModule.Data.Handlers
{
    public class SendNotificationsOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly INotificationManager _notificationManager;
        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;
        private readonly ISettingsManager _settingsManager;
        private readonly ISecurityService _securityService;

        public SendNotificationsOrderChangedEventHandler(INotificationManager notificationManager, IStoreService storeService, IMemberService memberService, ISettingsManager settingsManager, ISecurityService securityService)
        {
            _notificationManager = notificationManager;
            _storeService = storeService;
            _memberService = memberService;
            _settingsManager = settingsManager;
            _securityService = securityService;
        }


        public virtual async Task Handle(OrderChangedEvent message)
        {
            if (_settingsManager.GetValue("Order.SendOrderNotifications", true))
            {
                foreach (var changedEntry in message.ChangedEntries)
                {
                    await TryToSendOrderNotificationsAsync(changedEntry);
                }
            }
        }

        protected virtual async Task TryToSendOrderNotificationsAsync(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            //Collection of order notifications
            var notifications = new List<OrderEmailNotificationBase>();

            if (IsOrderCanceled(changedEntry))
            {
                var notification = _notificationManager.GetNewNotification<CancelOrderEmailNotification>(changedEntry.NewEntry.StoreId, "Store", changedEntry.NewEntry.LanguageCode);
                notifications.Add(notification);
            }

            if (changedEntry.EntryState == EntryState.Added && !changedEntry.NewEntry.IsPrototype)
            {
                var notification = _notificationManager.GetNewNotification<OrderCreateEmailNotification>(changedEntry.NewEntry.StoreId, "Store", changedEntry.NewEntry.LanguageCode);
                notifications.Add(notification);
            }

            if (HasNewStatus(changedEntry))
            {
                var notification = _notificationManager.GetNewNotification<NewOrderStatusEmailNotification>(changedEntry.NewEntry.StoreId, "Store", changedEntry.NewEntry.LanguageCode);

                notification.NewStatus = changedEntry.NewEntry.Status;
                notification.OldStatus = changedEntry.NewEntry.Status;
                notifications.Add(notification);
            }

            if (IsOrderPaid(changedEntry))
            {
                var notification = _notificationManager.GetNewNotification<OrderPaidEmailNotification>(changedEntry.NewEntry.StoreId, "Store", changedEntry.NewEntry.LanguageCode);
                notifications.Add(notification);
            }

            if (IsOrderSent(changedEntry))
            {
                var notification = _notificationManager.GetNewNotification<OrderSentEmailNotification>(changedEntry.NewEntry.StoreId, "Store", changedEntry.NewEntry.LanguageCode);
                notifications.Add(notification);
            }

            foreach (var notification in notifications)
            {
                notification.CustomerOrder = changedEntry.NewEntry;
                await SetNotificationParametersAsync(notification, changedEntry);
                _notificationManager.ScheduleSendNotification(notification);
            }
        }

        /// <summary>
        /// Is order canceled
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool IsOrderCanceled(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result =  !changedEntry.OldEntry.IsCancelled && changedEntry.NewEntry.IsCancelled;
            return result;
        }

        /// <summary>
        /// The order has a new status
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool HasNewStatus(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var retVal = changedEntry.OldEntry.Status != changedEntry.NewEntry.Status;
            return retVal;
        }

        /// <summary>
        /// Is order fully paid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool IsOrderPaid(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var oldPaidTotal = changedEntry.OldEntry.InPayments.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Sum);
            var newPaidTotal = changedEntry.NewEntry.InPayments.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Sum);            
            return oldPaidTotal != newPaidTotal && changedEntry.NewEntry.Total <= newPaidTotal;
        }

        /// <summary>
        /// Is order fully send
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool IsOrderSent(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var oldSentShipmentsCount = changedEntry.OldEntry.Shipments.Where(x => x.Status.EqualsInvariant("Send") || x.Status.EqualsInvariant("Sent")).Count();
            var newSentShipmentsCount = changedEntry.NewEntry.Shipments.Where(x => x.Status.EqualsInvariant("Send") || x.Status.EqualsInvariant("Sent")).Count();
            return oldSentShipmentsCount == 0 && newSentShipmentsCount > 0;
        }

        /// <summary>
        /// Set base notification parameters (sender, recipient, isActive)
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="changeEvent"></param>
        protected virtual async Task SetNotificationParametersAsync(Notification notification, GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var order = changedEntry.NewEntry;

            var store = _storeService.GetById(order.StoreId);
            notification.Sender = store.Email;
            notification.IsActive = true;
            notification.Recipient = await GetOrderRecipientEmailAsync(order);

            notification.ObjectTypeId = "CustomerOrder";
            notification.ObjectId = order.Id;        
            //Log all notification with subscription
            if (!string.IsNullOrEmpty(order.SubscriptionId))
            {
                notification.ObjectTypeId = "Subscription";
                notification.ObjectId = order.SubscriptionId;
            }
           
            var member = _memberService.GetByIds(new[] { order.CustomerId }).FirstOrDefault();
            if (member != null)
            {
                var email = member.Emails.FirstOrDefault();
                if (!string.IsNullOrEmpty(email))
                {
                    notification.Recipient = email;
                }
            }

            if (string.IsNullOrEmpty(notification.Recipient) && order.Addresses.Any())
            {
                var address = order.Addresses.FirstOrDefault();
                if (address != null)
                {
                    notification.Recipient = address.Email;
                }
            }
        }

        protected virtual async Task<string> GetOrderRecipientEmailAsync(CustomerOrder order)
        {
            //get recipient email from order address as default
            var email = order.Addresses?.Select(x => x.Email).FirstOrDefault();
            //try to find user
            var user = await _securityService.FindByIdAsync(order.CustomerId, UserDetails.Reduced);
            //Try to find contact 
            var contact = _memberService.GetByIds(new[] { order.CustomerId }).OfType<Contact>().FirstOrDefault();
            if (contact == null && user != null)
            {
                contact = _memberService.GetByIds(new[] { user.MemberId }).OfType<Contact>().FirstOrDefault();                
            }
            email = contact?.Emails?.FirstOrDefault() ?? email ?? user?.Email;
            return email;
        }
    }
}
