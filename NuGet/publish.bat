set V=2.10.4
nuget push VirtoCommerce.OrderModule.Client.%V%.nupkg -Source nuget.org -ApiKey %1
nuget push VirtoCommerce.OrderModule.Data.%V%.nupkg -Source nuget.org -ApiKey %1
pause