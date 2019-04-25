# VirtoCommerce Orders Module

## Overview

The Orders Module in Virto Commerce is a document based flexible orders management system with possibility to add unlimited number of documents related to customer order.

The Orders Module main purpose is to store order details and manage orders created by users on client side. This module is not designed to be a full order processing system like EPR but serves as storage for customer orders details and can be synchronized with different external processing systems.

The order itself contains minimum details, when the documents present additional order details, like payment, shipment, etc.  and display the order management lifecycle.

## Constraints

The Order Management process in Vitro Commerce OMS is not coded and not pre-determined. This system is designed as an Order Details Editor with no validation logics available. The system is implied to be an additional storage for customer orders details.

## Functional possibilities

Vitro Commers Orders Module supports the following functionalities:

1. Status update for each document type
2. Document based order structure. The Order contains related documents such as Payments, Shipments, Addresses, etc.
3. Ability to view and manage fulfillment, packages, pick-up and shipments documents
4. Dynamic extensibility of the Order Documents (possibility to add random fields)
5. Additional invoices
6. Saving Order drafts (postponed confirmation of order changes)
7. Changing Order products (quantity, product change, new products)
8. Changing of Order Product Price
9. Changing discounts
10. Add promotion coupons to Order
11. Managing financial documents
12. Refunds
13. Changing of Product items
14. Saving Order details change history (logs)
15. Saving payment details (cards, links, phone numbers)
16. Managing split shipments
17. Single shipment delivery of more than one order
18. Public API
    1. Search for orders by different criteria (customer, date, etc.). The system returns brief order details
    1. Manage order details
         1. Prices, products, coupons, delivery addresses, promotions, order status
         2. List of order related documents (order or payment cancelation, payment documents, shipment details, refund request, refunds, etc.). The document structure contains dynamically typed elements
    1. Order delivery (status, delivery details)
    1. Repeated order creation (order cloning) with possibility to specify the frequency of order re-creation

## Orders Module Structure

 ![Fig. Order Structure](/docs/media/diagram-order-module-structure.png)

## Customer orders

 The Customer orders screen can be accessed by navigating to Browse->Orders. The system displays the list of Customer orders. The user can select a Customer order from the list to view its details 
 ![Fig.Customer Orders](/docs/media/screen-customer-orders.png)

### Order Details

 Once the order is created by customer on client side, the details will immediately appear on admin side.
 ![Fig.Customer Orders](/docs/media/screen-order-details.png)

 The admin side displays the following order details:

1. Approved: Yes/No button
1. Assigned to: displays the name of the assignee. The admin can select the assignee from the drop-down list
1. Customer order Number: generated automatically by the system. The order number template can be specified by user
    1. The user navigates to settings->Orders->General 
    1. The system opens the General Settings screen
    1. The user specifies the Order number template 

![Fig.Customer Orders](/docs/media/screen-order-number-template.png)

1. From – date of creation
1. Status
1. Customer Name
1. Discount
1. Store

### Widgets 

![Fig.Customer Orders](/docs/media/screen-order-widgets.png)

1. Notification feed widget displays all notifications related to customer order 
1. Line Items widget display the list of selected items with details:
    1. Item
    1. Quantity
    1. Avail
    1. Unit price (excl. taxes)
    1. Unit price (incl. taxes)
    1. Discount (excl. taxes)
    1. Discount (incl. taxes)
    1. Tax
    1. Line items

![Fig.Customer Orders](/docs/media/screen-line-items.png)

1. Changes widget displays the order changes history (logs)
1. Addresses widget displays the shipment addresses specified by customer
1. Comments widget displays all comments related to
customer order
1. Dynamic properties  
1. Open Subscription

### Order Documents

Order documents are created by the system to store additional order details. There are two types of documents that are provided ‘out of the box’:

1. Payment document
1. Shipment document
The user can access the order documents by selecting a specific order. Order documents are displayed on order details screen

![Fig.Customer Orders](/docs/media/screen-order-documents1.png)

#### Order Payment Document

The user can view the payment details by clicking on the order payment document

![Fig.Customer Orders](/docs/media/screen-payment-document-details.png)

The  Payment document contains the following fields: 
1. Payment number generated automatically by the system. The order payment number template can be specified by user:
    1. The user navigates to settings->Orders->General 
    1. The system opens the General Settings screen
    1. The user specifies the Order payment number template 

![Fig.Customer Orders](/docs/media/screen-order-payment-number-template.png)

1. From label displays the order creation date 
1. Payment fees input field with currency as a label
1. Payment fees incl. taxes input field with currency as a label 
1. Status drop down 
1. Payment purpose input field
1. Amount input field 

The order payment document screen contains the following widgets: 

![Fig.Customer Orders](/docs/media/screen-payment-document-widgets.png)

1.	Comments – contains all comments related to order payments
2.	Payment address widget displays the billing address details 

a.	Address Name

b.	Address type: Shipping/ Billing and Shipping

c.	First Name

d.	Last Name

e.	Currency

f.	Region

g.	City

h.	Address 1

i.	Address 2

j.	Zip code

k.	Email 

l.	Phone 

3.	Transactions widget displays the list of all 
payment gateway transactions
4.	Totals widget displays the total payment details:

a.	Payment price

b.	Discount total

c.	Tax total 

d.	Total 

5.	Changes widget displays the payment changes history(logs) 
6.	Dynamic properties widget 

#### Order Shipment Document 

The user can view the Shipment document details by clicking on the ‘Shipment’ document 
The Order shipment document screen displays the following fields: 

![Fig.Customer Orders](/docs/media/screen-shipment-document.png)

1.	 Order Shipment number generated automatically by the system. The order shipment number template can be specified by user 

d.	The user navigates to settings->Orders->General 

e.	The system opens the General Settings screen

f.	The user specifies the Order shipment number 
template 

![Fig.Customer Orders](/docs/media/screen-order-shipment-number-template.png)

2.	From (label) displays the order creation date
3.	Status drop down displays the shipment status
4.	Assigned to drop down displays the name of the person responsible for the shipment processing 
5.	Shipment amount input field
6.	Shipment amount with tax
7.	Fulfillment center drop down 

![Fig.Customer Orders](/docs/media/screen-fullfilment-center.png)

The order shipment document screen displays the following widgets:
1.	Shipment items widget displays the list of items for shipping
2.	Changes widget displays the shipment changes history (logs) 
3.	Comments widget displays the comments related to the shipment 
4.	Dynamic properties widget
5.	Delivery address widget 
6.	Totals widget:

a.	Shipping price 

b.	Discount total

c.	Tax total

d.	Total 

## Roles and Permissions 

In order to manage Virto Commerce functionality each user must be assigned to at least one role. Each role provides the user with certain access permissions. These permissions allow or restrict the user's access to functionalities within the Virto Commerce client application. 
Permissions are controlled by assigning Roles to users. A Role is a collection of permissions. A Role can be assigned to multiple users. Each user can have more than one assigned Role.
Through the combination of assigned Roles, you can ensure that users only have access to the information and functionality they need.

### Scenarios 

In order to view and manage Orders in the ‘Orders Module’ the user shall be assigned to the ‘Order’ role with a pre-defined set of permissions. 

#### Add Order Role and Permissions 

![Fig.Customer Orders](/docs/media/diagram-add-roles-and-permissions.png)

1.	The admin navigates to Browse->Security->Roles 
2.	The system displays the ‘Roles’ screen
3.	The admin clicks the ‘Add’ button to create a new role
4.	The system displays the ‘New Role’ screen that contains the following elements: 

a.	Name input field

b.	Description input field 

c.	List of permissions. Each set of permissions contains:

i.	Permission name

ii.	Selectable list of permissions 

![Fig.Customer Orders](/docs/media/screen-permission-name.png)

![Fig.Customer Orders](/docs/media/screen-assign-permissions.png)

5.	The admin fills out the fields, assigns permissions to the new role and clicks the ‘Create’ button
6.	The new role with permissions is created 

**Important**: The admin can select different set of permissions for multiple roles related to Orders Module. Roles and permissions can be both edited and deleted from the system. 

#### Assign Order Role to User 

![Fig.Customer Orders](/docs/media/diagram-assign-role-with-permissions.png)

1.	The admin navigates to Browse->Security->Users and selects a user
2.	The system displays the user details screen
3.	The admin selects the Roles widget that can be either empty or may contain previously assigned roles to the selected user
4.	The admin clicks the ‘Assign’ button to assign a new role to the user
5.	The admin selects the role(s) and confirms the selection by clicking the ‘OK’ button 
6.	The system will assign the selected role to the user 

## Notification Templates

Each notification has its own template (NotificationTemplate) which is responsible for notification contents and consists of a view template with placeholders.

### Order Module Notification Types


The Orders Module provides the following Notification types:

#### Create Order Notification Template 

This notification can be used to notify the customer when a new order has been created. The notification is sent to customer via email 

![Fig.Customer Orders](/docs/media/screen-create-order-notification.png)

 #### Order Paid Notification Template 

This notification can be used to notify the customer that his orders are paid. The notification is sent to customer via email 

![Fig.Customer Orders](/docs/media/screen-order-paid-notification.png)

#### Order Sent Notification Template 

This notification is sent to customer via email when the status of the shipments changes to 'Sent' 

![Fig.Customer Orders](/docs/media/screen-order-sent-notification.png)


#### New Order Status Notification Template 

This notification is sent to customer via email when his order status was changed 

![Fig.Customer Orders](/docs/media/screen-new-order-status-notification.png)

#### Cancel Order Notification 

This notification can be sent via email to notify the customer that his order was canceled 

![Fig.Customer Orders](/docs/media/screen-cancel-order-notification.png)

#### Invoice for Customer Order 

This template is used for customer invoice generation in PDF format 

![Fig.Customer Orders](/docs/media/screen-invoice-for-customer-order.png)



# License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
