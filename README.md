# Virto Commerce Order Module

[![CI status](https://github.com/VirtoCommerce/vc-module-order/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-order/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order)

The Order module in Virto Commerce is a document based flexible orders management system with possibility to add unlimited number of documents related to customer order.

The Order module main purpose is to store order details and manage orders created by users on client side. This module is not designed to be a full order processing system like ERP but serves as storage for customer orders details and can be synchronized with different external processing systems.

The order itself contains minimum details, when the documents present additional order details, like payment, shipment, etc.  and display the order management life cycle.

The order management process in Vitro Commerce OMS is not coded and not pre-determined. This system is designed as an Order Details Editor with no validation logics available. The system is implied to be an additional storage for customer orders details.

## Key features

Virto Commerce Order module supports the following functionalities:

* Status update for each document type.
* Document based order structure. The order contains related documents such as Payments, Shipments, Addresses, etc. The order, being a document itself, is also a container for all documents related to the order processing: shipping, payment, custom documents. This approach allows mapping of supplier internal business processes of any complexity (multi-shipments, multi payments, specific inventory operations) to VirtoCommerce order structure. So it makes possible to keep track of documents begot by each order and show it to a customer if required.
* Ability to view and manage fulfillment, packages, pick-up and shipments documents.
* Dynamic extensibility of the 'Order Documents' (possibility to add custom fields). It is relatively easy to implement additional data for existent documents and new kinds of custom documents to the order container.
* Manage additional invoices.
* Save order drafts (postponed confirmation of order changes).
* Changing Order products (quantity, product change, new products).
* Possibility to make changes to order product price.
* Possibility to change discounts.
* Add promotion coupons to order.
* Payment history tracking. Orders contain document type "Payment". Using this type of documents allows keeping bills information and full logging of payment gateway transactions related to the order.
* Refunding possibilities.
* Possibility to change Product items.
* Save order details change history (logs).
* Save payment details (cards, links, phone numbers).
* Manage split shipments.
* Single shipment delivery of more than one order.
* Public API:
    * Search for orders by different criteria (customer, date, etc.). The system returns brief order details;
    * Manage order details;
         * Prices, products, coupons, delivery addresses, promotions, order status;
         * List of order related documents (order or payment cancellation, payment documents, shipment details, refund request, refunds, etc.). The document structure contains dynamically typed elements;
    * Manage order delivery (status, delivery details);
    * Repeated order creation (order cloning) with possibility to specify the frequency of order re-creation.

Number template format: `<template>@<reset_type>[:<start>:<increment>]`
- Reset types: `None`, `Daily`, `Weekly`, `Monthly`, `Yearly`
- Example: `CO{0:yyMMdd}-{1:D5}@Weekly:1:10` (weekly reset, start at 1, increment by 10)

#### Validation Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `Order.Validation.Enable` | `true` | Enable/disable order validation on save |
| `Order.MaxOrderDocumentCount` | `20` | Maximum child documents per order |

#### Feature Flags

| Setting | Default | Description |
|---------|---------|-------------|
| `Order.SendOrderNotifications` | `true` | Enable order status change notifications |
| `Order.AdjustInventory` | `false` | Adjust inventory on order status changes |
| `Order.LogOrderChanges` | `true` | Log changes to platform operation log |
| `Order.Search.EventBasedIndexation.Enable` | `false` | Auto-update search index on changes |
| `Order.PurchasedProductIndexation.Enable` | `false` | Enable purchased product indexing |
| `Order.OrderPaidAndOrderSentNotifications.Enable` | `false` | Use order paid/sent notifications |
| `Order.PaymentShipmentStatusChangedNotifications.Enable` | `false` | Use payment/shipment status notifications |

#### Dashboard Statistics Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `Order.DashboardStatistics.Enable` | `true` | Enable or disable order statistics widgets on the main dashboard |
| `Order.DashboardStatistics.RangeMonths` | `12` | Number of months to include in dashboard statistics calculations |

## Architecture

### Project Structure

```
vc-module-order/
??? src/
?   ??? VirtoCommerce.OrdersModule.Core/         # Domain models, interfaces
?   ??? VirtoCommerce.OrdersModule.Data/         # Data access, services, validators
?   ??? VirtoCommerce.OrdersModule.Data.SqlServer/   # SQL Server migrations
?   ??? VirtoCommerce.OrdersModule.Data.PostgreSql/  # PostgreSQL migrations
?   ??? VirtoCommerce.OrdersModule.Data.MySql/       # MySQL migrations
?   ??? VirtoCommerce.OrdersModule.Web/          # Web API, controllers
??? tests/
?   ??? VirtoCommerce.OrdersModule.Tests/        # Unit and integration tests
??? docs/                                        # Documentation
??? samples/                                     # Sample implementations
```

### Key Components

- **CustomerOrderService**: Core service for order CRUD operations
- **CustomerOrderSearchService**: Advanced search and filtering
- **CustomerOrderTotalsCalculator**: Order totals calculation
- **CustomerOrderValidator**: FluentValidation-based validation
- **OrderDocumentCountValidator**: Document count limit enforcement
- **OrderRepository**: Data access layer with EF Core

### Domain Model

```
CustomerOrder (IOperation)
??? LineItem[]
??? Address[]
??? PaymentIn[] (IOperation)
?   ??? Capture[]
?   ??? Refund[]
??? Shipment[] (IOperation)
?   ??? ShipmentItem[]
?   ??? ShipmentPackage[]
?   ??? Address
??? DynamicProperties[]
```



## Documentation

* [Order module user documentation](https://docs.virtocommerce.org/platform/user-guide/order-management/overview/)
* [GraphQL API documentation](https://docs.virtocommerce.org/platform/developer-guide/GraphQL-Storefront-API-Reference-xAPI/Order/overview/)
* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.Orders)
* [View on GitHub](https://github.com/VirtoCommerce/vc-module-order/)

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-order/releases/latest)

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
