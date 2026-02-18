# VirtoCommerce Orders Module - Project Structure

## Solution

**Solution file**: `VirtoCommerce.OrdersModule.sln`

## Directory Layout

```
vc-module-order/
├── src/
│   ├── VirtoCommerce.OrdersModule.Core/        # Domain models, interfaces, events
│   │   ├── Events/                              # Domain events
│   │   ├── Extensions/                          # Extension methods
│   │   ├── Model/                               # Domain models
│   │   ├── Notifications/                       # Notification definitions
│   │   ├── Search/                              # Search criteria and results
│   │   ├── Services/                            # Service interfaces
│   │   └── ModuleConstants.cs                   # Module constants, settings, permissions
│   │
│   ├── VirtoCommerce.OrdersModule.Data/        # Data access, services implementation
│   │   ├── Authorization/                       # Authorization handlers
│   │   ├── Caching/                             # Cache keys
│   │   ├── ExportImport/                        # Export/import functionality
│   │   ├── Extensions/                          # Extension methods
│   │   ├── Handlers/                            # Event handlers
│   │   ├── Model/                               # Entity models
│   │   ├── Repositories/                        # Data repositories
│   │   ├── Search/                              # Search service implementations
│   │   ├── Services/                            # Service implementations
│   │   └── Validators/                          # FluentValidation validators
│   │
│   ├── VirtoCommerce.OrdersModule.Data.SqlServer/    # SQL Server migrations
│   ├── VirtoCommerce.OrdersModule.Data.PostgreSql/   # PostgreSQL migrations
│   ├── VirtoCommerce.OrdersModule.Data.MySql/        # MySQL migrations
│   │
│   └── VirtoCommerce.OrdersModule.Web/         # Web API, module entry point
│       ├── Authorization/                       # Web authorization
│       ├── Content/                             # Static content
│       ├── Controllers/                         # REST API controllers
│       ├── dist/                                # Compiled frontend
│       ├── Extensions/                          # Web extensions
│       ├── JsonConverters/                      # Custom JSON converters
│       ├── Localizations/                       # Localization files
│       ├── NotificationTemplates/               # Email templates
│       ├── Scripts/                             # AngularJS frontend
│       ├── Module.cs                            # Module entry point
│       └── module.manifest                      # Module metadata
│
├── tests/
│   └── VirtoCommerce.OrdersModule.Tests/       # Unit and integration tests
│
├── docs/                                        # Documentation
├── samples/                                     # Sample implementations
├── .nuke/                                       # NUKE build configuration
├── .github/                                     # GitHub workflows
├── Directory.Build.props                        # Shared build properties
└── .editorconfig                                # Code style configuration
```

## Key Files

| File | Purpose |
|------|---------|
| `src/VirtoCommerce.OrdersModule.Web/Module.cs` | Module entry point, dependency registration |
| `src/VirtoCommerce.OrdersModule.Web/module.manifest` | Module metadata, version, dependencies |
| `src/VirtoCommerce.OrdersModule.Core/ModuleConstants.cs` | Settings, permissions, event types |
| `Directory.Build.props` | Shared build configuration, version |
| `.editorconfig` | Code style rules |

## Database Migrations

Migrations are database-specific and located in:
- SQL Server: `src/VirtoCommerce.OrdersModule.Data.SqlServer/`
- PostgreSQL: `src/VirtoCommerce.OrdersModule.Data.PostgreSql/`
- MySQL: `src/VirtoCommerce.OrdersModule.Data.MySql/`
