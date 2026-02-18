# VirtoCommerce Orders Module - Project Overview

## Purpose

The VirtoCommerce Order Module is a document-based flexible order management system for the VirtoCommerce e-commerce platform. It handles complex order processing workflows including payments, shipments, refunds, and custom document types.

## Key Features

- **Document-based structure**: Orders contain related documents (payments, shipments, addresses)
- **Status management**: Independent status tracking for each document type
- **Order editing**: Modify products, quantities, prices, discounts
- **Draft orders**: Save and manage order drafts
- **Order cloning**: Create repeated orders
- **Change history**: Complete audit trail of modifications
- **Dynamic properties**: Extend orders with custom fields
- **Number templates**: Configurable order/payment/shipment number generation
- **FluentValidation**: Validation rules for order documents
- **Search indexing**: Advanced search with event-based indexation
- **Email notifications**: Configurable templates for order lifecycle events
- **Role-based access control**: Granular permissions for order operations

## Tech Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 8.0 |
| Platform | VirtoCommerce 3.x (platform version 3.917.0+) |
| Database | SQL Server, PostgreSQL, MySQL |
| ORM | Entity Framework Core |
| Validation | FluentValidation |
| Testing | xUnit, MSTest, Moq, FluentAssertions |
| Code Coverage | coverlet |
| Frontend | AngularJS (VirtoCommerce Admin UI) |
| Build | NUKE |

## Module Version

Current version: 3.867.0

## Dependencies

| Module | Version |
|--------|---------|
| VirtoCommerce.Assets | 3.814.0 |
| VirtoCommerce.Cart | 3.841.0+ |
| VirtoCommerce.Catalog | 3.913.0+ |
| VirtoCommerce.Core | 3.824.0 |
| VirtoCommerce.Customer | 3.846.0 |
| VirtoCommerce.Inventory | 3.815.0 |
| VirtoCommerce.Notifications | 3.830.0 |
| VirtoCommerce.Payment | 3.811.0 |
| VirtoCommerce.Search | 3.821.0 |
| VirtoCommerce.Shipping | 3.816.0 |
| VirtoCommerce.Store | 3.823.0 |

## Domain Model

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

## Key Services

- **CustomerOrderService**: Core service for order CRUD operations
- **CustomerOrderSearchService**: Advanced search and filtering
- **CustomerOrderTotalsCalculator**: Order totals calculation
- **CustomerOrderValidator**: FluentValidation-based validation
- **OrderDocumentCountValidator**: Document count limit enforcement
- **OrderRepository**: Data access layer with EF Core
