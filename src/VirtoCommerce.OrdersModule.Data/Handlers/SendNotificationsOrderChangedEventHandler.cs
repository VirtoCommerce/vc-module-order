using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.OrdersModule.Core;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Notifications;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.PaymentModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.OrdersModule.Data.Handlers
{
    public class SendNotificationsOrderChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly INotificationSearchService _notificationSearchService;
        private readonly INotificationSender _notificationSender;
        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;
        private readonly ISettingsManager _settingsManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICustomerOrderService _orderService;

        public SendNotificationsOrderChangedEventHandler(
            INotificationSender notificationSender,
            IStoreService storeService,
            IMemberService memberService,
            ISettingsManager settingsManager,
            UserManager<ApplicationUser> userManager,
            INotificationSearchService notificationSearchService,
            ICustomerOrderService orderService)
        {
            _notificationSender = notificationSender;
            _storeService = storeService;
            _memberService = memberService;
            _settingsManager = settingsManager;
            _notificationSearchService = notificationSearchService;
            _userManager = userManager;
            _orderService = orderService;
        }

        public virtual async Task Handle(OrderChangedEvent message)
        {
            if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.SendOrderNotifications))
            {
                var jobArguments = message.ChangedEntries.SelectMany(GetJobArgumentsForChangedEntry).ToArray();
                if (jobArguments.Any())
                {
                    BackgroundJob.Enqueue(() => TryToSendOrderNotificationsAsync(jobArguments));
                }
            }
        }

        protected virtual OrderNotificationJobArgument[] GetJobArgumentsForChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = new List<OrderNotificationJobArgument>();

            if (IsOrderCanceled(changedEntry))
            {
                result.Add(OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(CancelOrderEmailNotification)));
            }

            if (IsNewlyAdded(changedEntry))
            {
                result.Add(OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(OrderCreateEmailNotification)));
            }

            if (HasNewStatus(changedEntry))
            {
                result.Add(OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(NewOrderStatusEmailNotification)));
            }

            if (_settingsManager.GetValue<bool>(ModuleConstants.Settings.General.PaymentShipmentStatusChangedNotifications))
            {
                var changedPayments = GetEntriesWithChangedStatus(changedEntry.OldEntry.InPayments, changedEntry.NewEntry.InPayments);
                if (changedPayments.Any())
                {
                    var argument = OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(PaymentStatusChangedEmailNotification));
                    argument.OrderOperations = changedPayments;
                    result.Add(argument);
                }

                var changedShipments = GetEntriesWithChangedStatus(changedEntry.OldEntry.Shipments, changedEntry.NewEntry.Shipments);
                if (changedShipments.Any())
                {
                    var argument = OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(ShipmentStatusChangedEmailNotification));
                    argument.OrderOperations = changedShipments;
                    result.Add(argument);
                }
            }

            if (_settingsManager.GetValue<bool>(ModuleConstants.Settings.General.OrderPaidAndOrderSentNotifications))
            {
                if (IsOrderPaid(changedEntry))
                {
                    result.Add(OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(OrderPaidEmailNotification)));
                }

                if (IsOrderSent(changedEntry))
                {
                    result.Add(OrderNotificationJobArgument.FromChangedEntry(changedEntry, typeof(OrderSentEmailNotification)));
                }
            }

            return result.ToArray();
        }

        public virtual async Task TryToSendOrderNotificationsAsync(OrderNotificationJobArgument[] jobArguments)
        {
            var ordersByIdDict = (await _orderService.GetAsync(jobArguments.Select(x => x.CustomerOrderId).Distinct().ToList()))
                                .ToDictionary(x => x.Id)
                                .WithDefaultValue(null);

            foreach (var jobArgument in jobArguments)
            {
                var notification = await _notificationSearchService.GetNotificationAsync(jobArgument.NotificationTypeName, new TenantIdentity(jobArgument.StoreId, nameof(Store)));
                if (notification != null)
                {
                    var order = ordersByIdDict[jobArgument.CustomerOrderId];

                    if (order != null && notification is OrderEmailNotificationBase orderNotification)
                    {
                        var customer = await GetCustomerAsync(jobArgument.CustomerId);

                        orderNotification.CustomerOrder = order;
                        orderNotification.Customer = customer;
                        orderNotification.LanguageCode = order.LanguageCode;

                        await SetNotificationParametersAsync(notification, order);

                        if (notification is NewOrderStatusEmailNotification newStatusNotification)
                        {
                            newStatusNotification.OldStatus = jobArgument.OldStatus;
                            newStatusNotification.NewStatus = jobArgument.NewStatus;
                        }

                        if (notification is PaymentStatusChangedEmailNotification paymentsStatusesChangedEmailNotification)
                        {
                            paymentsStatusesChangedEmailNotification.Entries = jobArgument.OrderOperations;
                        }

                        if (notification is ShipmentStatusChangedEmailNotification shipmentsStatusesChangedEmailNotification)
                        {
                            shipmentsStatusesChangedEmailNotification.Entries = jobArgument.OrderOperations;
                        }

                        await _notificationSender.ScheduleSendNotificationAsync(notification);
                    }
                }
            }
        }

        protected virtual bool IsNewlyAdded(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = changedEntry.EntryState == EntryState.Added && !changedEntry.NewEntry.IsPrototype;
            return result;
        }

        /// <summary>
        /// Is order canceled
        /// </summary>
        /// <param name="changedEntry"></param>
        /// <returns></returns>
        protected virtual bool IsOrderCanceled(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = !changedEntry.OldEntry.IsCancelled && changedEntry.NewEntry.IsCancelled;
            return result;
        }

        /// <summary>
        /// The order has a new status
        /// </summary>
        /// <param name="changedEntry"></param>
        /// <returns></returns>
        protected virtual bool HasNewStatus(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var result = changedEntry.OldEntry.Status != changedEntry.NewEntry.Status;
            return result;
        }

        protected virtual IList<OrderOperationStatusChangedEntry> GetEntriesWithChangedStatus<T>(ICollection<T> oldEntries, ICollection<T> newEntries)
            where T : OrderOperation
        {
            var documents = new List<OrderOperationStatusChangedEntry>();
            if (!oldEntries.Any() || !newEntries.Any())
            {
                return documents;
            }
            documents.AddRange(
                from oldInPayment in oldEntries
                let newInPayment = newEntries.FirstOrDefault(x => x.Id == oldInPayment.Id && x.Status != oldInPayment.Status)
                where newInPayment != null
                select new OrderOperationStatusChangedEntry
                {
                    Number = newInPayment.Number,
                    OldStatus = oldInPayment.Status,
                    NewStatus = newInPayment.Status,
                });

            return documents;
        }

        /// <summary>
        /// Is order fully paid
        /// </summary>
        /// <param name="changedEntry"></param>
        /// <returns></returns>
        protected virtual bool IsOrderPaid(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var oldPaidTotal = changedEntry.OldEntry.InPayments?.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Sum) ?? 0m;
            var newPaidTotal = changedEntry.NewEntry.InPayments?.Where(x => x.PaymentStatus == PaymentStatus.Paid).Sum(x => x.Sum) ?? 0m;
            return oldPaidTotal != newPaidTotal && changedEntry.NewEntry.Total <= newPaidTotal;
        }

        /// <summary>
        /// Is order fully send
        /// </summary>
        /// <param name="changedEntry"></param>
        /// <returns></returns>
        protected virtual bool IsOrderSent(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var oldSentShipmentsCount = changedEntry.OldEntry.Shipments?.Count(x => x.Status.EqualsInvariant("Send") || x.Status.EqualsInvariant("Sent")) ?? 0;
            var newSentShipmentsCount = changedEntry.NewEntry.Shipments?.Count(x => x.Status.EqualsInvariant("Send") || x.Status.EqualsInvariant("Sent")) ?? 0;
            return oldSentShipmentsCount == 0 && newSentShipmentsCount > 0;
        }

        /// <summary>
        /// Set base notification parameters (sender, recipient, isActive)
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="order"></param>
        protected virtual async Task SetNotificationParametersAsync(Notification notification, CustomerOrder order)
        {
            if (notification is EmailNotification emailNotification)
            {
                var store = await _storeService.GetNoCloneAsync(order.StoreId, StoreResponseGroup.StoreInfo.ToString());
                emailNotification.From = store?.EmailWithName;
                emailNotification.To = await GetOrderRecipientEmailAsync(order);
            }

            // Allow to filter notification log either by customer order or by subscription
            if (string.IsNullOrEmpty(order.SubscriptionId))
            {
                notification.TenantIdentity = new TenantIdentity(order.Id, nameof(CustomerOrder));
            }
            else
            {
                notification.TenantIdentity = new TenantIdentity(order.SubscriptionId, "Subscription");
            }
        }

        protected virtual async Task<string> GetOrderRecipientEmailAsync(CustomerOrder order)
        {
            var email = GetOrderAddressEmail(order) ?? await GetCustomerEmailAsync(order.CustomerId);
            return email;
        }

        protected virtual string GetOrderAddressEmail(CustomerOrder order)
        {
            var email = order.Addresses?.Select(x => x.Email).FirstOrDefault(x => !string.IsNullOrEmpty(x));
            return email;
        }

        protected virtual async Task<string> GetCustomerEmailAsync(string customerId)
        {
            var customer = await GetCustomerAsync(customerId);

            if (customer == null)
            {
                // try to find user
                var user = await _userManager.FindByIdAsync(customerId);

                return user?.Email;
            }

            return customer.Emails?.FirstOrDefault();
        }

        protected virtual async Task<Member> GetCustomerAsync(string customerId)
        {
            // Try to find contact
            var result = await _memberService.GetByIdAsync(customerId);

            if (result == null)
            {
                var user = await _userManager.FindByIdAsync(customerId);

                if (user != null)
                {
                    result = await _memberService.GetByIdAsync(user.MemberId);
                }
            }

            return result;
        }
    }

    public class OrderNotificationJobArgument
    {
        public string NotificationTypeName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerOrderId { get; set; }
        public string StoreId { get; set; }
        public string NewStatus { get; set; }
        public string OldStatus { get; set; }
        public IList<OrderOperationStatusChangedEntry> OrderOperations { get; set; }

        public static OrderNotificationJobArgument FromChangedEntry(GenericChangedEntry<CustomerOrder> changedEntry, Type notificationType)
        {
            var result = new OrderNotificationJobArgument
            {
                CustomerOrderId = changedEntry.NewEntry?.Id ?? changedEntry.OldEntry?.Id,
                NotificationTypeName = notificationType.Name,
                StoreId = changedEntry.NewEntry.StoreId,
                CustomerId = changedEntry.NewEntry.CustomerId,
                NewStatus = changedEntry.NewEntry.Status,
                OldStatus = changedEntry.OldEntry.Status,
            };

            return result;
        }
    }
}
