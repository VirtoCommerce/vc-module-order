# VirtoCommerce Orders Module

## Overview

The Orders Module in Virto Commerce is a document based flexible orders management system with possibility to add unlimited number of documents related to customer order.

The Orders Module main purpose is to store order details and manage orders created by users on client side. This module is not designed to be a full order processing system like EPR but serves as storage for customer orders details and can be synchronized with different external processing systems.

The order itself contains minimum details, when the documents present additional order details, like payment, shipment, etc.  and display the order management life cycle.

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

1. [Order Details](/docs/order-details.md)

1. [Widgets](/docs/widgets.md)
1. [Order Documents](/docs/order-documents.md)



## Roles and Permissions

In order to manage Virto Commerce functionality each user must be assigned to at least one role. Each role provides the user with certain access permissions. These permissions allow or restrict the user's access to functionalities within the Virto Commerce client application.
Permissions are controlled by assigning Roles to users. A Role is a collection of permissions. A Role can be assigned to multiple users. Each user can have more than one assigned Role.
Through the combination of assigned Roles, you can ensure that users only have access to the information and functionality they need.

[User Scenarios](/docs/roles-and-permissions-scenarios.ms)



## Notification Templates

Each notification has its own template (NotificationTemplate) which is responsible for notification contents and consists of a view template with placeholders.

[Order Module Notification Types](/docs/order-module-notification-types.md)



## Database Model

![Fig.db-model](/docs/media/diagram-db-model.png)

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
