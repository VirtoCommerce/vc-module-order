using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.OrderModule.Data.Notifications
{
    public abstract class OrderEmailNotificationBase : EmailNotification, INotificationParameterInitializable
    {
        public OrderEmailNotificationBase(IEmailNotificationSendingGateway gateway) : base(gateway) { }

        [NotificationParameter("Customer Order")]
        public CustomerOrder CustomerOrder { get; set; }

        public virtual void Initialize(IServiceProvider serviceProvider, NotificationParameter[] notificationParameters)
        {
            foreach (var notificationParameter in notificationParameters)
            {
                switch (notificationParameter.ParameterName)
                {
                    case "CustomerOrder":
                        var searchService = serviceProvider.GetService(typeof(ICustomerOrderSearchService)) as ICustomerOrderSearchService;
                        var orderSearchResult = searchService.SearchCustomerOrders(new CustomerOrderSearchCriteria
                        {
                            Number = (string)notificationParameter.Value,
                            Take = 1
                        });
                        CustomerOrder = orderSearchResult.Results.FirstOrDefault();
                        break;

                    case "Recipient":
                        Recipient = (string)notificationParameter.Value;
                        break;

                    case "Sender":
                        Sender = (string)notificationParameter.Value;
                        break;
                }
            }
            if (CustomerOrder != null)
            {
                if (String.IsNullOrEmpty(Recipient))
                {
                    var memberService = serviceProvider.GetService(typeof(IMemberService)) as IMemberService;
                    var contact = memberService.GetByIds(new[] { CustomerOrder.CustomerId }).OfType<Contact>().FirstOrDefault();
                    Recipient = contact?.Emails.FirstOrDefault();
                }
                if (String.IsNullOrEmpty(Sender))
                {
                    var storeService = serviceProvider.GetService(typeof(IStoreService)) as IStoreService;
                    var store = storeService.GetById(CustomerOrder.StoreId);
                    Sender = store?.Email;
                }
            }
        }
    }
}
