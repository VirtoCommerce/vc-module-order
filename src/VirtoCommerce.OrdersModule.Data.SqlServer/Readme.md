
## Package manager 
Add-Migration Initial -Context VirtoCommerce.OrdersModule.Data.Repositories.OrderDbContext  -Verbose -OutputDir Migrations -Project VirtoCommerce.OrdersModule.Data.SqlServer -StartupProject VirtoCommerce.OrdersModule.Data.SqlServer  -Debug



### Entity Framework Core Commands
```

dotnet tool install --global dotnet-ef --version 6.*
```

**Generate Migrations**

```
dotnet ef migrations add Initial -- "{connection string}"
dotnet ef migrations add Update1 -- "{connection string}"
dotnet ef migrations add Update2 -- "{connection string}"
```

etc..

**Apply Migrations**

`dotnet ef database update -- "{connection string}"`