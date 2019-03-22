using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Order.Events;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Order.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.OrderModule.Core.Services;
using VirtoCommerce.OrderModule.Data.Notifications;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.OrderModule.Data.Handlers
{
    public class SendNotificationsOrderWorkflowChangedEventHandler : IEventHandler<OrderChangedEvent>
    {
        private readonly INotificationManager _notificationManager;
        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;
        private readonly ISettingsManager _settingsManager;
        private readonly ISecurityService _securityService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IWorkflowPermissionService _workflowPermissionService;

        public SendNotificationsOrderWorkflowChangedEventHandler(INotificationManager notificationManager, IStoreService storeService, IMemberService memberService, ISettingsManager settingsManager, ISecurityService securityService, ICustomerOrderService customerOrderService, IMemberSearchService memberSearchService, IWorkflowPermissionService workflowPermissionService)
        {
            _notificationManager = notificationManager;
            _storeService = storeService;
            _memberService = memberService;
            _settingsManager = settingsManager;
            _securityService = securityService;
            _memberSearchService = memberSearchService;
            _workflowPermissionService = workflowPermissionService;
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
            if (changedEntry.EntryState == EntryState.Added && !changedEntry.NewEntry.IsPrototype)
            {
                var notifications = new List<OrderEmailNotificationBase>();
                var emailManagerList = await GetManagerEmails(changedEntry);

                foreach (var email in emailManagerList)
                {
                    var notificationWorkflow = _notificationManager.GetNewNotification<OrderWorkflowNotification>(changedEntry.NewEntry.StoreId, "Store", changedEntry.NewEntry.LanguageCode);
                    notificationWorkflow.CustomerOrder = changedEntry.NewEntry;
                    SetNotificationParameters(notificationWorkflow, changedEntry, email);

                    notifications.Add(notificationWorkflow);
                    _notificationManager.ScheduleSendNotification(notificationWorkflow);
                }
            }
        }

        private async Task<List<string>> GetManagerEmails(GenericChangedEntry<CustomerOrder> changedEntry)
        {
            var currentUser = await _securityService.FindByIdAsync(changedEntry.NewEntry.CustomerId, UserDetails.Reduced);
           
            var emplopyee = _memberService.GetByIds(new[] { currentUser.MemberId }).OfType<Employee>().FirstOrDefault();
            
            var contact = _memberService.GetByIds(new[] { currentUser.MemberId }).OfType<Contact>().FirstOrDefault();

            var searchCriteria = new MembersSearchCriteria
            {
                DeepSearch = true,
                Take = int.MaxValue,
                MemberId = emplopyee == null ? contact.Organizations.FirstOrDefault(): emplopyee.Organizations.FirstOrDefault()
            };
            var membersOfContact = _memberSearchService.SearchMembers(searchCriteria);
            var membersWithSecurityInfo = membersOfContact.Results.OfType<IHasSecurityAccounts>();

            var managerPermission = _workflowPermissionService.ManagerPermission;
            var emailManagerList = new List<string>();

            foreach (var user in membersWithSecurityInfo)
            {
                var fullUserDetail = await _securityService.FindByIdAsync(user.SecurityAccounts.FirstOrDefault().Id, UserDetails.Full);
                if (fullUserDetail.Permissions.Contains(managerPermission))
                {
                    emailManagerList.AddDistinct<string>(fullUserDetail.Email);
                }
            }

            return emailManagerList;
        }

        /// <summary>
        /// Set base notification parameters (sender, recipient, isActive)
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="changedEntry"></param>
        protected virtual void SetNotificationParameters(Notification notification, GenericChangedEntry<CustomerOrder> changedEntry, string recipientEmail)
        {
            var order = changedEntry.NewEntry;
            var store = _storeService.GetById(order.StoreId);

            notification.IsActive = true;

            notification.Sender = store.Email;
            notification.Recipient = recipientEmail;

            // Allow to filter notification log either by customer order or by subscription
            if (string.IsNullOrEmpty(order.SubscriptionId))
            {
                notification.ObjectTypeId = "CustomerOrder";
                notification.ObjectId = order.Id;
            }
            else
            {
                notification.ObjectTypeId = "Subscription";
                notification.ObjectId = order.SubscriptionId;
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
            var user = await _securityService.FindByIdAsync(customerId, UserDetails.Reduced);

            var contact = user != null
                ? _memberService.GetByIds(new[] { user.MemberId }).OfType<Contact>().FirstOrDefault()
                : _memberService.GetByIds(new[] { customerId }).OfType<Contact>().FirstOrDefault();

            var email = contact?.Emails?.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? user?.Email;
            return email;
        }
    }
}
