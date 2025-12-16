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

## Extension Points

### Custom Validators

Create custom validators by implementing `IValidator<IOperation>`:

```csharp
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.Platform.Core.Settings;

public class CustomOrderValidator : AbstractValidator<IOperation>
{
    public CustomOrderValidator(ISettingsManager settingsManager)
    {
        RuleFor(operation => operation)
            .CustomAsync(async (operation, context, cancellationToken) =>
            {
                // Validate only root-level operations
                if (!string.IsNullOrEmpty(operation.ParentOperationId))
                    return;

                // Your custom validation logic
                if (SomeCondition(operation))
                {
                    context.AddFailure("Your error message");
                }
            });
    }
}
```

Register in `ServiceCollectionExtensions.cs`:

```csharp
serviceCollection.AddTransient<IValidator<IOperation>, CustomOrderValidator>();
```

### Custom Document Types

Extend orders with custom document types:

```csharp
public class CustomDocument : OrderOperation
{
    public string CustomField { get; set; }
    public DateTime CustomDate { get; set; }
    
    // Override base methods as needed
}

public class CustomDocumentEntity : OperationEntity
{
    public string CustomField { get; set; }
    public DateTime CustomDate { get; set; }
}
```

### Event Handlers

Subscribe to order events:

```csharp
public class OrderEventHandler : INotificationHandler<OrderChangedEvent>
{
    public async Task Handle(OrderChangedEvent notification, CancellationToken cancellationToken)
    {
        foreach (var changedEntry in notification.ChangedEntries)
        {
            if (changedEntry.EntryState == EntryState.Added)
            {
                // Handle new order
            }
            else if (changedEntry.EntryState == EntryState.Modified)
            {
                // Handle order update
            }
        }
    }
}
```

### Custom Totals Calculator

Implement custom totals calculation logic:

```csharp
public class CustomTotalsCalculator : ICustomerOrderTotalsCalculator
{
    public void CalculateTotals(CustomerOrder order)
    {
        // Custom calculation logic
    }
}
```

## Usage Scenarios

### Scenario 1: System Administrator - Configure Document Limits

**Goal**: Prevent performance issues by limiting child documents per order.

**Steps**:
1. Navigate to **Settings → Orders → General**
2. Set **Order.MaxOrderDocumentCount** to `20` (or desired limit)
3. Enable **Order.Validation.Enable**
4. Save settings

**Result**: System validates document counts on save and displays clear error messages:
```
CustomerOrder document count (25) exceeds the maximum allowed limit of 20.
Documents breakdown: PaymentIn=15, Shipment=10
```

### Scenario 2: Developer - Implement Custom Business Rules

**Goal**: Add business-specific validation (e.g., orders over $10,000 require approval).

**Steps**:
1. Create validator implementing `AbstractValidator<IOperation>`
2. Add validation rules using FluentValidation syntax
3. Register validator in DI container
4. Validation runs automatically on order save

**Benefits**:
- Centralized validation logic
- Consistent enforcement across all channels (UI, API, integrations)
- Clear error messages for users

### Scenario 3: Integration Developer - Sync with External ERP

**Goal**: Keep order data synchronized with external ERP system using event-driven integration.

#### Option A: Direct Event Subscription (In-Process)

**Steps**:
1. Subscribe to `OrderChangedEvent` in your custom module
2. Implement event handler to push changes to ERP
3. Use order search API to fetch order details
4. Handle ERP responses and update order status

**Benefits**:
- Immediate processing within the same process
- Direct access to platform services
- Simple implementation for single-instance deployments

#### Option B: Webhooks Module (Recommended for External Systems)

**Integration**: Install [vc-module-webhooks](https://github.com/VirtoCommerce/vc-module-webhooks)

**Steps**:
1. Install Webhooks module from Virto Commerce Marketplace
2. Navigate to **Settings → Webhooks** in Admin Portal
3. Create a new webhook subscription:
   - **Event**: Select `OrderChangedEvent`
   - **URL**: Your ERP webhook endpoint (e.g., `https://erp.company.com/api/webhooks/orders`)
   - **Authentication**: Configure Basic or Bearer Token authentication
   - **Payload**: Select fields to include in notification
4. Configure retry policy with exponential backoff
5. Monitor webhook delivery status and error messages

**Benefits**:
- No custom code required
- Built-in retry mechanism with exponential intervals
- Authentication support (Basic & Bearer Token)
- Field-level control over payload
- Background processing doesn't block order operations
- View error logs when notifications fail
- Perfect for microservices and external integrations

**Example Webhook Payload**:
```json
{
  "eventId": "order-changed-123",
  "eventType": "OrderChangedEvent",
  "timestamp": "2025-01-15T10:30:00Z",
  "data": {
    "orderId": "ORDER-001",
    "customerId": "customer-123",
    "status": "Processing",
    "total": 1250.00,
    "currency": "USD",
    "changedEntries": [
      {
        "entryState": "Modified",
        "oldEntry": { "status": "New" },
        "newEntry": { "status": "Processing" }
      }
    ]
  }
}
```

#### Option C: Event Bus Module (Recommended for Message Queues)

**Integration**: Install [vc-module-event-bus](https://github.com/VirtoCommerce/vc-module-event-bus)

**Steps**:
1. Install Event Bus module from Virto Commerce Marketplace
2. Configure destination provider in `appsettings.json`:
   ```json
   {
     "EventBus": {
       "Provider": "AzureEventGrid",
       "AzureEventGrid": {
         "TopicEndpoint": "https://your-topic.eventgrid.azure.net/api/events",
         "AccessKey": "your-access-key"
       },
       "Subscriptions": [
         {
           "EventType": "OrderChangedEvent",
           "FilterExpression": "$.Data.Total > 100",
           "PayloadTransformation": "liquid-template"
         }
       ]
     }
   }
   ```
3. Set up Azure Event Grid topic and subscriptions
4. Configure ERP system to consume events from message queue
5. Optionally use JsonPath filters to process only specific events
6. Apply Liquid templates to transform payload for ERP format

**Benefits**:
- CloudEvents-based standard format
- Enterprise message queue integration (Azure Event Grid, etc.)
- High performance and scalability
- Advanced filtering with JsonPath expressions
- Payload transformation with Liquid templates
- Decoupled architecture for reactive programming
- Support for multiple destination providers
- Perfect for event-driven microservices architecture

**Example CloudEvents Payload**:
```json
{
  "specversion": "1.0",
  "type": "VirtoCommerce.OrderChangedEvent",
  "source": "https://store.virtocommerce.com/orders",
  "id": "A234-1234-1234",
  "time": "2025-01-15T10:30:00Z",
  "datacontenttype": "application/json",
  "data": {
    "orderId": "ORDER-001",
    "status": "Processing",
    "total": 1250.00
  }
}
```

#### Comparison Matrix

| Feature | Direct Events | Webhooks Module | Event Bus Module |
|---------|--------------|-----------------|------------------|
| **Deployment** | In-process | Any architecture | Message queue required |
| **Code Required** | Custom handler | No code | Configuration only |
| **Reliability** | Process-dependent | Built-in retry | Message queue guarantees |
| **Scalability** | Limited | Good | Excellent |
| **Filtering** | Code-based | Field selection | JsonPath expressions |
| **Transformation** | Code-based | Field selection | Liquid templates |
| **Authentication** | Custom | Basic/Bearer | Provider-specific |
| **Monitoring** | Custom logging | UI dashboard | Provider monitoring |
| **Best For** | Simple integrations | REST API endpoints | Enterprise message buses |

#### Recommended Approach

For **production ERP integration**, use:
- **Webhooks Module** if your ERP exposes REST API webhooks endpoints
- **Event Bus Module** if you have enterprise message queue infrastructure (Azure Event Grid, AWS EventBridge, etc.)
- **Direct Events** only for simple, single-instance deployments or when you need direct access to platform services

**Hybrid Approach**: Use Event Bus for real-time order notifications + Webhooks for specific ERP operations that require REST API calls.

### Scenario 4: Store Manager - Bulk Order Import

**Goal**: Import historical orders without validation overhead.

**Steps**:
1. Disable **Order.Validation.Enable** temporarily
2. Run bulk import process
3. Re-enable **Order.Validation.Enable**

**Result**: Faster imports, validation enforced for new orders.

## Testing

### Run Unit Tests

```bash
dotnet test --filter "Category=Unit"
```

### Run Integration Tests

```bash
dotnet test --filter "Category=IntegrationTest"
```

### Test Coverage

The module includes comprehensive test coverage:
- **Unit tests**: 26+ tests covering validation, service logic, and business rules
- **Integration tests**: Database operations, repository patterns, and service integration
- **Validator tests**: Both synchronous and asynchronous validation paths

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
