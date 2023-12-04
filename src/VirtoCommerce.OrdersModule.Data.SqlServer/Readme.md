# Entity Framework Core Commands

## Installation
```
dotnet tool install --global dotnet-ef --version 6.*
```

## Generate Migrations

```
dotnet ef migrations add Initial
dotnet ef migrations add Update1
dotnet ef migrations add Update2
```

### Apply Migrations

`dotnet ef database update`
