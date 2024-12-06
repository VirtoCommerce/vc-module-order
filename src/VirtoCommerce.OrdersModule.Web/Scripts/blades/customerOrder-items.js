angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemsController', [
        '$scope', '$translate',
        'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        'virtoCommerce.orderModule.catalogItems', 'virtoCommerce.orderModule.prices',
        function ($scope, $translate, bladeNavigationService, authService, items, prices) {
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

            $scope.openLineItemDetail = function (item, index) {
                blade.selectedNodeId = index;
                var newBlade = {
                    id: "listLineItemDetail",
                    controller: 'virtoCommerce.orderModule.customerOrderItemDetailController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-detail.tpl.html',
                    title: item.name,
                    currentEntityId: item.id,
                    order: blade.currentEntity,
                    recalculateFn: blade.recalculateFn,
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
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    title: "orders.blades.catalog-items-select.title",
                    currentEntities: blade.currentEntity,
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
                        var selectedLineItems = _.filter(lineItems, function (x) { return x.selected; });

                        if (blade.selectedNodeId >= 0) {
                            var selectedNode = lineItems[blade.selectedNodeId];
                            if (selectedNode && _.some(selectedLineItems, lineItem => selectedNode.id === lineItem.id)) {
                                bladeNavigationService.closeChildrenBlades(blade);
                            }
                        }

                        blade.currentEntity.items = _.difference(lineItems, selectedLineItems);
                        blade.selectedAll = false;
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

            $scope.updateSelectedAll = function () {
                blade.selectedAll = _.every(blade.currentEntity.items, item => item.selected);
            };

            blade.refresh();
        }]);
