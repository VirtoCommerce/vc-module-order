
## Package manager 
Add-Migration Initial -Context VirtoCommerce.OrdersModule.Data.Repositories.OrderDbContext  -Verbose -OutputDir Migrations -Project VirtoCommerce.OrdersModule.Data.MySql -StartupProject VirtoCommerce.OrdersModule.Data.MySql  -Debug



### Entity Framework Core Commands
```
dotnet tool install --global dotnet-ef --version 6.*
```

**Generate Migrations**

```
dotnet ef migrations add Initial
dotnet ef migrations add Update1
dotnet ef migrations add Update2
```

etc..

**Apply Migrations**

`dotnet ef database update`
