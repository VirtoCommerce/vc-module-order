angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.shipmentItemsController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.authService', function ($scope, bladeNavigationService, dialogService, authService) {
        var blade = $scope.blade;
        blade.updatePermission = 'order:update';
        blade.isVisiblePrices = authService.checkPermission('order:read_prices');

        blade.currentEntity.items = blade.currentEntity.items || [];

        var selectedNode = null;
        var selectedProducts = [];


        blade.refresh = function () {
            blade.isLoading = false;
            blade.selectedAll = false;
        };

        blade.toolbarCommands = [
            {
                name: "orders.commands.add-item", icon: 'fa fa-plus',
                executeMethod: function () {
                    openAddEntityWizard();
                },
                canExecuteMethod: function () {
                    return blade.currentEntity.operationType === 'Shipment';
                },
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.remove", icon: 'fa fa-trash-o',
                executeMethod: function () {
                    var items = blade.currentEntity.items;
                    blade.currentEntity.items = _.difference(items, _.filter(items, function (x) { return x.selected }));

                },
                canExecuteMethod: function () {
                    return _.any(blade.currentEntity.items, function (x) { return x.selected; });
                },
                permission: blade.updatePermission
            }
        ];

        $scope.selectItem = function (node) {
            selectedNode = node;
            $scope.selectedNodeId = selectedNode.id;
        };

        $scope.checkAll = function (selected) {
            angular.forEach(blade.currentEntity.items, function (item) {
                item.selected = selected;
            });
        };

        function openAddEntityWizard() {
            var options = {
                checkItemFn: function (listItem, isSelected) {
                    if (isSelected) {
                        if (_.all(selectedProducts, function (x) { return x.id != listItem.id; })) {
                            selectedProducts.push(listItem);
                        }
                    }
                    else {
                        selectedProducts = _.reject(selectedProducts, function (x) { return x.id == listItem.id; });
                    }
                }
            };
            var newBlade = {
                id: "OrderItemsSelect",
                currentEntity: blade.currentEntity,
                orderItems: blade.parentBlade.parentBlade.currentEntity.items,
                title: "orders.blades.catalog-items-select.title",
                controller: 'virtoCommerce.orderModule.orderItemSelectController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-items-select.tpl.html',
                options: options,
                breadcrumbs: [],
                toolbarCommands: [
                    {
                        name: "orders.commands.add-selected", icon: 'fa fa-plus',
                        executeMethod: function (blade) {
                            //addProductsToOrder(selectedProducts);
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

        blade.refresh();
    }]);
