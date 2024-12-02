angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemDetailController', [
        '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        function ($scope, bladeNavigationService, authService) {
            var blade = $scope.blade;
            blade.updatePermission = 'order:update';
            blade.isVisiblePrices = authService.checkPermission('order:read_prices');
            blade.title = blade.currentEntity.name;
            blade.metaFields1 = [
                {
                    title: "orders.blades.customerOrder-item-detail.labels.product",
                    colSpan: 2,
                    templateUrl: "name.html"
                },
                {
                    templateUrl: "status.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.quantity",
                    templateUrl: "quantity.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.price",
                    templateUrl: "price.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.priceWithTax",
                    templateUrl: "priceWithTax.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.discount",
                    templateUrl: "discountAmount.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.discountWithTax",
                    templateUrl: "discountAmountWithTax.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.tax",
                    templateUrl: "taxTotal.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.total",
                    templateUrl: "extendedPriceWithTax.html"
                }
            ];

//            $translate('orders.blades.customerOrder-detail.title', { customer: blade.currentEntity.customerName }).then(function (result) {
//                blade.title = 'orders.widgets.customerOrder-items.blade-title';
//                blade.titleValues = { title: result };
//                blade.subtitle = 'orders.widgets.customerOrder-items.blade-subtitle';
//            });

            blade.refresh = function () {
                blade.isLoading = false;
            };

            blade.openStatusSettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html',
                    currentEntityId: 'OrderLineItem.Statuses',
                    isApiSave: true,
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

//            $scope.openItemDynamicProperties = function (item) {
//                var blade = {
//                    id: "dynamicPropertiesList",
//                    controller: 'platformWebApp.propertyValueListController',
//                    template: '$(Platform)/Scripts/app/dynamicProperties/blades/propertyValue-list.tpl.html',
//                    currentEntity: item,
//                };
//                bladeNavigationService.showBlade(blade, $scope.blade);
//            };
//
            blade.openItemDetail = function () {
                var newBlade = {
                    id: "listItemDetail",
                    controller: 'virtoCommerce.catalogModule.itemDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html',
                    title: blade.currentEntity.name,
                    itemId: blade.currentEntity.productId,
                    productType: blade.currentEntity.productType
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };
            
//            blade.toolbarCommands = [
//                {
//                    name: "orders.commands.add-item", icon: 'fas fa-plus',
//                    executeMethod: function () {
//                        openAddEntityWizard();
//                    },
//                    canExecuteMethod: function () {
//                        return blade.currentEntity.operationType === 'CustomerOrder';
//                    },
//                    permission: blade.updatePermission
//                },
//                {
//                    name: "platform.commands.remove", icon: 'fas fa-trash-alt',
//                    executeMethod: function () {
//                        var lineItems = blade.currentEntity.items;
//                        blade.currentEntity.items = _.difference(lineItems, _.filter(lineItems, function (x) { return x.selected }));
//                        blade.recalculateFn();
//                    },
//                    canExecuteMethod: function () {
//                        return _.any(blade.currentEntity.items, function (x) { return x.selected; });
//                    },
//                    permission: blade.updatePermission
//                }
//            ];

            blade.refresh();
        }]);
