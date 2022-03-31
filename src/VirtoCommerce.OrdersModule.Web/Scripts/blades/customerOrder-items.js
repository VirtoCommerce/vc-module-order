angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemsController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.catalogItems', 'virtoCommerce.orderModule.prices', '$translate', 'platformWebApp.authService', function ($scope, bladeNavigationService, items, prices, $translate, authService) {
    var blade = $scope.blade;
    blade.updatePermission = 'order:update';
    blade.isVisiblePrices = authService.checkPermission('order:read_prices');

    var selectedProducts = [];

    $translate('orders.blades.customerOrder-detail.title', { customer: blade.currentEntity.customerName }).then(function (result) {
        blade.title = 'orders.widgets.customerOrder-items.blade-title';
        blade.titleValues = { title: result };
        blade.subtitle = 'orders.widgets.customerOrder-items.blade-subtitle';
    });

    blade.refresh = function () {
        blade.isLoading = false;
        blade.selectedAll = false;
    };

    function addProductsToOrder(products) {
        angular.forEach(products, function (product) {
            items.get({ id: product.id }, function (data) {
                prices.getProductPrices({ id: product.id }, function (prices) {
                    var price = _.find(prices, function (x) { return x.currency === blade.currentEntity.currency });

                    var newLineItem =
                    {
                        productId: data.id,
                        catalogId: data.catalogId,
                        categoryId: data.categoryId,
                        name: data.name,
                        imageUrl: data.imgSrc,
                        sku: data.code,
                        quantity: 1,
                        price: price && price.list ? price.list : 0,
                        discountAmount: price && price.list && price.sale ? price.list - price.sale : 0,
                        currency: blade.currentEntity.currency
                    };
                    blade.currentEntity.items.push(newLineItem);
                    blade.recalculateFn();
                }, function (error) {
                    if (error.status == 404) {
                        // Seems no pricing module installed.
                        // Just add lineitem with zero price.
                        var newLineItem =
                        {
                            productId: data.id,
                            catalogId: data.catalogId,
                            categoryId: data.categoryId,
                            name: data.name,
                            imageUrl: data.imgSrc,
                            sku: data.code,
                            quantity: 1,
                            price: 0,
                            discountAmount: 0,
                            currency: blade.currentEntity.currency
                        };
                        blade.currentEntity.items.push(newLineItem);
                        blade.recalculateFn();
                    }
                    
                });
            });
        });
    };

    $scope.openItemDynamicProperties = function (item) {
        var blade = {
            id: "dynamicPropertiesList",
            currentEntity: item,
            controller: 'platformWebApp.propertyValueListController',
            template: '$(Platform)/Scripts/app/dynamicProperties/blades/propertyValue-list.tpl.html'
        };
        bladeNavigationService.showBlade(blade, $scope.blade);
    };

    $scope.openItemDetail = function (item) {
        var newBlade = {
            id: "listItemDetail",
            itemId: item.productId,
            productType: item.productType,
            title: item.name,
            controller: 'virtoCommerce.catalogModule.itemDetailController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    };

    function openAddEntityWizard() {
        var options = {
            checkItemFn: function (listItem, isSelected) {
                if (isSelected) {
                    if (_.all(selectedProducts, function (x) { return x.id !== listItem.id; })) {
                        selectedProducts.push(listItem);
                    }
                }
                else {
                    selectedProducts = _.reject(selectedProducts, function (x) { return x.id === listItem.id; });
                }
            }
        };
        var newBlade = {
            id: "CatalogItemsSelect",
            currentEntities: blade.currentEntity,
            title: "orders.blades.catalog-items-select.title",
            controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
            options: options,
            breadcrumbs: [],
            toolbarCommands: [
              {
                  name: "orders.commands.add-selected", icon: 'fas fa-plus',
                  executeMethod: function (blade) {
                      addProductsToOrder(selectedProducts);
                      selectedProducts.length = 0;
                      bladeNavigationService.closeBlade(blade);
                  },
                  canExecuteMethod: function () {
                      return selectedProducts.length > 0;
                  }
              }]
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    }

    blade.toolbarCommands = [
        {
            name: "orders.commands.add-item", icon: 'fas fa-plus',
            executeMethod: function () {
                openAddEntityWizard();
            },
            canExecuteMethod: function () {
                return blade.currentEntity.operationType === 'CustomerOrder';
            },
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.remove", icon: 'fas fa-trash-alt',
            executeMethod: function () {
                var lineItems = blade.currentEntity.items;
                blade.currentEntity.items = _.difference(lineItems, _.filter(lineItems, function (x) { return x.selected }));
                blade.recalculateFn();
            },
            canExecuteMethod: function () {
                return _.any(blade.currentEntity.items, function (x) { return x.selected; });
            },
            permission: blade.updatePermission
        }
    ];

    $scope.checkAll = function (selected) {
        angular.forEach(blade.currentEntity.items, function (item) {
            item.selected = selected;
        });
    };


    blade.refresh();
}]);
