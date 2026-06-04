angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemConfigurationProductsController', [
        '$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.productBladeResolver',
        function ($scope, uiGridHelper, bladeNavigationService, productBladeResolver) {
            var blade = $scope.blade;
            blade.title = 'orders.blades.customerOrder-item-configuration.menu.products.title';
            blade.headIcon = 'fas fa-box';

            blade.toolbarCommands = [
                {
                    name: "platform.navigation.back",
                    icon: 'fas fa-arrow-left',
                    canExecuteMethod: function () { return true; },
                    executeMethod: function () {
                        var newBlade = {
                            id: "itemConfiguration",
                            controller: 'virtoCommerce.orderModule.customerOrderItemConfigurationController',
                            template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-configuration.tpl.html',
                            currentEntity: blade.currentEntity,
                        };
                        bladeNavigationService.showBlade(newBlade, $scope.blade.parentBlade);
                    },
                }
            ];

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                });
            };

            $scope.selectNode = function (item) {
                $scope.selectedNodeId = item.id;

                return productBladeResolver.open({
                    blade: blade,
                    item: item,
                });
            }

            function initialize() {
                blade.isLoading = false;
                blade.items = blade.currentEntity.configurationItems.filter(x => x.type === 'Product');
            }

            initialize();
        }]);
