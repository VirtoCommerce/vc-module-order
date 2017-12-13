using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Notifications;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrderModule.Data.Observers
{
    public class OrderNotificationObserver : IObserver<OrderChangedEvent>
    {
        private readonly INotificationManager _notificationManager;
        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;
        private readonly ISettingsManager _settingsManager;

        public OrderNotificationObserver(INotificationManager notificationManager, IStoreService storeService, IMemberService memberService, ISettingsManager settingsManager)
        {
            _notificationManager = notificationManager;
            _storeService = storeService;
            _memberService = memberService;
            _settingsManager = settingsManager;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(OrderChangedEvent value)
        {
            if (_settingsManager.GetValue("Order.SendOrderNotifications", true))
            {
                //Collection of order notifications
                var notifications = new List<OrderEmailNotificationBase>();

                if (IsOrderCanceled(value))
                {
                    var notification = _notificationManager.GetNewNotification<CancelOrderEmailNotification>(value.ModifiedOrder.StoreId, "Store", value.ModifiedOrder.LanguageCode);
                    notifications.Add(notification);
                }

                if (value.ChangeState == EntryState.Added && !value.ModifiedOrder.IsPrototype)
                {
                    var notification = _notificationManager.GetNewNotification<OrderCreateEmailNotification>(value.ModifiedOrder.StoreId, "Store", value.ModifiedOrder.LanguageCode);
                    notifications.Add(notification);
                }

                if (IsNewStatus(value))
                {
                    var notification = _notificationManager.GetNewNotification<NewOrderStatusEmailNotification>(value.ModifiedOrder.StoreId, "Store", value.ModifiedOrder.LanguageCode);

                    notification.NewStatus = value.ModifiedOrder.Status;
                    notification.OldStatus = value.OrigOrder.Status;

                    notifications.Add(notification);
                }

                if (IsOrderPaid(value))
                {
                    var notification = _notificationManager.GetNewNotification<OrderPaidEmailNotification>(value.ModifiedOrder.StoreId, "Store", value.ModifiedOrder.LanguageCode);
                    notifications.Add(notification);
                }

                if (IsOrderSent(value))
                {
                    var notification = _notificationManager.GetNewNotification<OrderSentEmailNotification>(value.ModifiedOrder.StoreId, "Store", value.ModifiedOrder.LanguageCode);
                    notifications.Add(notification);
                }

                foreach (var notification in notifications)
                {
                    notification.CustomerOrder = value.ModifiedOrder;
                    SetNotificationParameters(notification, value);
                    _notificationManager.ScheduleSendNotification(notification);
                }
            }
        }


        /// <summary>
        /// Is order was canceled
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsOrderCanceled(OrderChangedEvent value)
        {
            var retVal = value.OrigOrder != null &&
                          value.OrigOrder.IsCancelled != value.ModifiedOrder.IsCancelled &&
                          value.ModifiedOrder.IsCancelled;

            return retVal;
        }

        /// <summary>
        /// Is order gets new status
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsNewStatus(OrderChangedEvent value)
        {
            var retVal = value.OrigOrder != null &&
                          value.OrigOrder.Status != value.ModifiedOrder.Status;

            return retVal;
        }

        /// <summary>
        /// Is order fully paid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsOrderPaid(OrderChangedEvent value)
        {
            var retVal = false;

            foreach (var origPayment in value.OrigOrder.InPayments)
            {
                var modifiedPayment = value.ModifiedOrder.InPayments.FirstOrDefault(i => i.Id == origPayment.Id);
                if (modifiedPayment != null)
                {
                    var paidSum = value.ModifiedOrder.InPayments.Where(i => i.PaymentStatus == PaymentStatus.Paid).Sum(i => i.Sum);
                    retVal = modifiedPayment.PaymentStatus == PaymentStatus.Paid && origPayment.PaymentStatus != PaymentStatus.Paid && paidSum == value.ModifiedOrder.Total;
                }

                if (retVal)
                {
                    break;
                }
            }

            return retVal;
        }

        /// <summary>
        /// Is order fully send
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool IsOrderSent(OrderChangedEvent value)
        {
            var retVal = false;

            foreach (var origShipment in value.OrigOrder.Shipments)
            {
                var modifiedShipment = value.ModifiedOrder.Shipments.FirstOrDefault(i => i.Id == origShipment.Id);
                if (modifiedShipment != null)
                {
                    retVal = (modifiedShipment.Status != origShipment.Status && modifiedShipment.Status == "Send") || (retVal && modifiedShipment.Status == "Send");
                }
            }

            return retVal;
        }

        /// <summary>
        /// Set base notificaiton parameters (sender, recipient, isActive)
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="changeEvent"></param>
        private void SetNotificationParameters(Notification notification, OrderChangedEvent changeEvent)
        {
            var order = changeEvent.ModifiedOrder;

            var store = _storeService.GetById(order.StoreId);
            notification.Sender = store.Email;
            notification.IsActive = true;

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
    }
}
