# Virto Commerce Order Module

[![CI status](https://github.com/VirtoCommerce/vc-module-order/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-order/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-order&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-order)

## Overview

The Virto Commerce Order Module is a flexible, document-based order management system designed to handle complex order processing workflows. Built on a hierarchical document structure, the module enables businesses to manage orders with unlimited related documents including payments, shipments, refunds, and custom document types.

### Architecture

The module implements a document-centric architecture where:
- **Orders** serve as containers for related operational documents
- **Documents** (payments, shipments, captures, refunds) represent the order lifecycle stages
- **Hierarchical structure** supports nested operations through the `IOperation` interface
- **Extensibility** allows adding custom document types and fields dynamically

### Core Principles

- **Not a full ERP system**: Designed as an order details storage and editor, synchronized with external processing systems
- **Flexible validation**: Configurable validation rules that can be enabled or disabled based on business requirements
- **Performance optimization**: Built-in limits and constraints ensure system stability at scale
- **Document lifecycle**: Track the complete order management process through status updates and document changes

## Key Features

### Order Management

* **Document-based structure**: Orders contain related documents (payments, shipments, addresses) that map to any business process complexity
* **Status management**: Independent status tracking for each document type in the order hierarchy
* **Order editing**: Modify products, quantities, prices, discounts, and add promotion coupons
* **Draft orders**: Save and manage order drafts with postponed confirmation
* **Order cloning**: Create repeated orders with configurable frequency
* **Change history**: Complete audit trail of all order modifications
* **Dynamic properties**: Extend orders with custom fields and metadata
* **Number templates**: Configurable order, payment, shipment, and refund number generation with counter reset options

### Document Management

* **Payment documents**: Track payment history, gateway transactions, and billing information
  - Support for multiple payment methods per order
  - Payment captures and authorization tracking
  - Payment status lifecycle (New → Authorized → Paid → Refunded)
* **Shipment documents**: Manage fulfillment, packages, pick-up, and delivery details
  - Multiple shipping methods support
  - Shipment status tracking (New → PickPack → ReadyToSend → Sent)
  - Split shipments and partial fulfillment
* **Refund documents**: Handle refund requests and processing with status tracking
* **Capture documents**: Track payment capture operations for authorized payments
* **Custom documents**: Extend with additional document types and dynamic fields
* **Invoice management**: Generate and manage PDF invoices

### Validation and Constraints

* **Configurable validation**: Enable or disable validation rules through settings (sync/async support)
* **Document count limits**: Define maximum number of child documents per order (default: 20)
  - Prevents performance degradation
  - Ensures storage optimization
  - Maintains data consistency
  - Validation at both order-level and per-document-level
* **Hierarchical validation**: Validates entire operation tree using `IOperation` interface
* **Graceful error handling**: Clear exception messages with detailed document breakdown
* **FluentValidation**: Built on FluentValidation framework for extensible validation rules
* **Custom validators**: Easy to add business-specific validation logic

### Search and Indexing

* **Advanced search**: Search orders by customer, date, status, store, total, and custom criteria
* **Event-based indexing**: Automatic search index updates on order changes
* **Purchased product tracking**: Index purchased products for analytics and reporting
* **Store-specific filters**: Filter purchased products by store

### API and Integration

* **REST API**: Full CRUD operations for orders and documents
* **GraphQL API**: Modern query interface for storefront integration
* **Search capabilities**: Advanced search with pagination and filtering
* **External system sync**: Designed for integration with ERP and other processing systems
* **Event-based architecture**: Publish domain events for order changes and lifecycle transitions
  - `OrderChangeEvent` / `OrderChangedEvent`
  - `OrderPaymentStatusChangedEvent`
  - `OrderShipmentStatusChangedEvent`
* **Integration modules** (optional, install separately):
  - **[Webhooks Module](https://github.com/VirtoCommerce/vc-module-webhooks)**: Send order events to external REST APIs with retry logic and authentication
  - **[Event Bus Module](https://github.com/VirtoCommerce/vc-module-event-bus)**: Publish events to message queues (Azure Event Grid) using CloudEvents format

### Notifications

* **Email notifications**: Configurable email templates for order lifecycle events
  - Order created
  - Order status changed
  - Order cancelled
  - Order paid
  - Order sent
  - Payment status changed
  - Shipment status changed
* **Invoice generation**: PDF invoice generation with customizable templates
* **Localization**: Multi-language support for notifications (10 languages)

### Security and Permissions

* **Role-based access control**: Granular permissions for order operations
  - `order:read` - View orders
  - `order:create` - Create new orders
  - `order:update` - Modify existing orders
  - `order:delete` - Delete orders
  - `order:access` - Access order module
  - `order:read_prices` - View order prices
  - `order:update_shipments` - Manage shipments
  - `order:capture_payment` - Capture payments
  - `order:refund` - Process refunds
  - `order:dashboardstatistics:view` - View dashboard statistics
* **Scope-based access**: Restrict access by store or organization

## Configuration

### Required Settings

Navigate to **Settings → Orders → General** in the Admin Portal:

#### Core Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `Order.Status` | See list | Available order statuses |
| `Order.InitialStatus` | `New` | Initial status for new orders |
| `Order.InitialProcessingStatus` | `Processing` | Status for orders with terminated payment |
| `OrderLineItem.Statuses` | See list | Available line item statuses |
| `Shipment.Status` | `New` | Available shipment statuses |
| `PaymentIn.Status` | `New` | Available payment statuses |
| `Refund.Status` | `Pending` | Available refund statuses |

#### Number Generation

| Setting | Default | Description |
|---------|---------|-------------|
| `Order.CustomerOrderNewNumberTemplate` | `CO{0:yyMMdd}-{1:D5}` | Order number template with counter |
| `Order.ShipmentNewNumberTemplate` | `SH{0:yyMMdd}-{1:D5}` | Shipment number template |
| `Order.PaymentInNewNumberTemplate` | `PI{0:yyMMdd}-{1:D5}` | Payment number template |
| `Order.RefundNewNumberTemplate` | `RF{0:yyMMdd}-{1:D5}` | Refund number template |

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
├── src/
│   ├── VirtoCommerce.OrdersModule.Core/         # Domain models, interfaces
│   ├── VirtoCommerce.OrdersModule.Data/         # Data access, services, validators
│   ├── VirtoCommerce.OrdersModule.Data.SqlServer/   # SQL Server migrations
│   ├── VirtoCommerce.OrdersModule.Data.PostgreSql/  # PostgreSQL migrations
│   ├── VirtoCommerce.OrdersModule.Data.MySql/       # MySQL migrations
│   └── VirtoCommerce.OrdersModule.Web/          # Web API, controllers
├── tests/
│   └── VirtoCommerce.OrdersModule.Tests/        # Unit and integration tests
├── docs/                                        # Documentation
└── samples/                                     # Sample implementations
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
├── LineItem[]
├── Address[]
├── PaymentIn[] (IOperation)
│   ├── Capture[]
│   └── Refund[]
├── Shipment[] (IOperation)
│   ├── ShipmentItem[]
│   ├── ShipmentPackage[]
│   └── Address
└── DynamicProperties[]
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

Copyright (c) Virto Solutions LTD. All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
