# Overview

[![CI status](https://github.com/VirtoCommerce/vc-module-order/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-order/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order)

The Orders Module in VirtoCommerce is a document based flexible orders management system with possibility to add unlimited number of documents related to customer order.

The Orders Module main purpose is to store order details and manage orders created by users on client side. This module is not designed to be a full order processing system like ERP but serves as storage for customer orders details and can be synchronized with different external processing systems.

The order itself contains minimum details, when the documents present additional order details, like payment, shipment, etc.  and display the order management life cycle.

The Order Management process in VitroCommerce OMS is not coded and not pre-determined. This system is designed as an Order Details Editor with no validation logics available. The system is implied to be an additional storage for customer orders details.

## Key Features

VirtoCommerce Orders Module supports the following functionalities:

1. Status update for each document type.
2. Document based order structure. The order contains related documents such as Payments, Shipments, Addresses, etc. The order, being a document itself, is also a container for all documents related to the order processing: shipping, payment, custom documents. This approach allows mapping of supplier internal business processes of any complexity (multi-shipments, multi payments, specific inventory operations) to VirtoCommerce order structure. So it makes possible to keep track of documents begot by each order and show it to a customer if required.
3. Ability to view and manage fulfillment, packages, pick-up and shipments documents.
4. Dynamic extensibility of the 'Order Documents' (possibility to add custom fields). It is relatively easy to implement additional data for existent documents and new kinds of custom documents to the order container.
5. Manage additional invoices.
6. Save order drafts (postponed confirmation of order changes).
7. Changing Order products (quantity, product change, new products).
8. Possibility to make changes to order product price.
9. Possibility to change discounts.
10. Add promotion coupons to order.
11. Payment history tracking. Orders contain document type "Payment". Using this type of documents allows keeping bills information and full logging of payment gateway transactions related to the order.
12. Refunding possibilities.
13. Possibility to change Product items.
14. Save order details change history (logs).
15. Save payment details (cards, links, phone numbers).
16. Manage split shipments.
17. Single shipment delivery of more than one order.
18. Public API:
    1. Search for orders by different criteria (customer, date, etc.). The system returns brief order details;
    1. Manage order details;
         1. Prices, products, coupons, delivery addresses, promotions, order status;
         2. List of order related documents (order or payment cancellation, payment documents, shipment details, refund request, refunds, etc.). The document structure contains dynamically typed elements;
    1. Manage order delivery (status, delivery details);
    1. Repeated order creation (order cloning) with possibility to specify the frequency of order re-creation.

## Documentation

* [Order Module Document](https://docs.virtocommerce.org/platform/user-guide/order-management/overview/)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-order/tree/dev)

## References

* Deploy: https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/
* Installation: https://docs.virtocommerce.org/platform/user-guide/modules-installation/
Home: https://virtocommerce.com
* Community: https://www.virtocommerce.org
* [Download Latest Release](https://github.com/VirtoCommerce/vc-module-order/releases/latest)

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
