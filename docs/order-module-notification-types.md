# Notification Types

Each notification in VirtoCommerce Platform has its own template (NotificationTemplate) which is responsible for notification contents and consists of a view template with placeholders.
Different markup languages (Razor, Liquid etc) can be used for view templates. In this implementation we chose to use liquidtemplate syntax and templates created with it are processed using a dotliquid library (a .net library for a popular ruby view engine).
There is also built in support for a separate template per language and instance of the object. For example, you can define different template for the same order confirmation notification per shop and language.

The Orders Module contains the following Notification template types:

1. **Create Order Notification Template** .
This notification can be used to notify the customer when a new order has been created. The notification is sent to customer via email.
![Fig.Customer Orders](/docs/media/screen-create-order-notification.png)
1. **Order Paid Notification Template**.
This notification can be used to notify the customer that his orders are paid. The notification is sent to customer via email
![Fig.Customer Orders](/docs/media/screen-order-paid-notification.png)
1. **Order Sent Notification Template**.
This notification is sent to customer via email when all his shipments are in ‘Sent’ status.
![Fig.Customer Orders](/docs/media/screen-order-sent-notification.png)
1. **New Order Status Notification Template**.
This notification is sent to customer via email when his order status was changed.
![Fig.Customer Orders](/docs/media/screen-new-order-status-notification.png)
1. **Cancel Order Notification**.
This notification can be sent via email to notify the customer that his order was cancelled.
![Fig.Customer Orders](/docs/media/screen-cancel-order-notification.png)
1. **Invoice for Customer order**
![Fig.Customer Orders](/docs/media/screen-invoice-for-customer-order.png)