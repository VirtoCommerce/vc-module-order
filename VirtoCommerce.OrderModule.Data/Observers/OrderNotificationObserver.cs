using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Data.Notifications;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrderModule.Data.Observers
{
    public class OrderNotificationObserver : IObserver<OrderChangedEvent>
    {
        protected readonly INotificationManager NotificationManager;
        protected readonly IStoreService StoreService;
        protected readonly IMemberService MemberService;
        protected readonly ISettingsManager SettingsManager;

        public OrderNotificationObserver(INotificationManager notificationManager, IStoreService storeService, IMemberService memberService,
            ISettingsManager settingsManager)
        {
            NotificationManager = notificationManager;
            StoreService = storeService;
            MemberService = memberService;
            SettingsManager = settingsManager;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(OrderChangedEvent value)
        {
            if (SettingsManager.GetValue("Order.SendOrderNotifications", true))
            {
                //Collection of order notifications
                var notifications = new List<OrderEmailNotificationBase>();

                if (IsOrderCanceled(value))
                {
                    OnOrderIsCancelled(value, notifications);
                }

                if (value.ChangeState == EntryState.Added && !value.ModifiedOrder.IsPrototype)
                {
                    OnOrderCreated(value, notifications);
                }

                if (IsNewStatus(value))
                {
                    OnOrderStatusChanged(value, notifications);
                }

                if (IsOrderPaid(value))
                {
                    OnOrderPaid(value, notifications);
                }

                if (IsOrderSent(value))
                {
                    var notification = NotificationManager.GetNewNotification<OrderSentEmailNotification>(value.ModifiedOrder.StoreId, "Store", value.ModifiedOrder.LanguageCode);
                    if (notification != null)
                    {
                        notifications.Add(notification);
                    }
                }

                foreach (var notification in notifications)
                {
                    notification.CustomerOrder = value.ModifiedOrder;
                    SetNotificationParameters(notification, value);
                    NotificationManager.ScheduleSendNotification(notification);
                }
            }
        }

        protected virtual void OnOrderIsCancelled(OrderChangedEvent value, List<OrderEmailNotificationBase> notifications)
        {
            var notification = NotificationManager.GetNewNotification<CancelOrderEmailNotification>(value.ModifiedOrder.StoreId, "Store",
                value.ModifiedOrder.LanguageCode);
            if (notification != null)
            {
                notifications.Add(notification);
            }
        }

        protected virtual void OnOrderCreated(OrderChangedEvent value, List<OrderEmailNotificationBase> notifications)
        {
            var notification = NotificationManager.GetNewNotification<OrderCreateEmailNotification>(value.ModifiedOrder.StoreId, "Store",
                value.ModifiedOrder.LanguageCode);
            if (notification != null)
            {
                notifications.Add(notification);
            }
        }

        protected virtual void OnOrderStatusChanged(OrderChangedEvent value, List<OrderEmailNotificationBase> notifications)
        {
            var notification = NotificationManager.GetNewNotification<NewOrderStatusEmailNotification>(value.ModifiedOrder.StoreId, "Store",
                value.ModifiedOrder.LanguageCode);

            if (notification != null)
            {
                notification.NewStatus = value.ModifiedOrder.Status;
                notification.OldStatus = value.OrigOrder.Status;

                notifications.Add(notification);
            }
        }

        protected virtual void OnOrderSent(OrderChangedEvent value, List<OrderEmailNotificationBase> notifications)
        {
            var notification = NotificationManager.GetNewNotification<OrderSentEmailNotification>(value.ModifiedOrder.StoreId, "Store",
                value.ModifiedOrder.LanguageCode);
            if (notification != null)
            {
                notifications.Add(notification);
            }
        }

        protected virtual void OnOrderPaid(OrderChangedEvent value, List<OrderEmailNotificationBase> notifications)
        {
            var notification = NotificationManager.GetNewNotification<OrderPaidEmailNotification>(value.ModifiedOrder.StoreId, "Store",
                value.ModifiedOrder.LanguageCode);
            if (notification != null)
            {
                notifications.Add(notification);
            }
        }

        /// <summary>
        /// Is order was canceled
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool IsOrderCanceled(OrderChangedEvent value)
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
        protected virtual bool IsNewStatus(OrderChangedEvent value)
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
        protected virtual bool IsOrderPaid(OrderChangedEvent value)
        {
            return value.IsOrderNowPaid();
        }

        /// <summary>
        /// Is order fully send
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool IsOrderSent(OrderChangedEvent value)
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
        protected virtual void SetNotificationParameters(Notification notification, OrderChangedEvent changeEvent)
        {
            var order = changeEvent.ModifiedOrder;

            var store = StoreService.GetById(order.StoreId);
            notification.Sender = store.Email;
            notification.IsActive = true;

            //Log all notification with subscription
            if (!string.IsNullOrEmpty(order.SubscriptionId))
            {
                notification.ObjectTypeId = "Subscription";
                notification.ObjectId = order.SubscriptionId;
            }

            var member = MemberService.GetByIds(new[] { order.CustomerId }).FirstOrDefault();
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
