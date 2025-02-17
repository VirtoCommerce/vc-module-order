angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemConfigurationTextsController', [
        '$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService',
        function ($scope, uiGridHelper, bladeNavigationService) {
            var blade = $scope.blade;
            blade.title = 'orders.blades.customerOrder-item-configuration.menu.texts.title';
            blade.headIcon = 'fa fa-file-text';

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

            function initialize() {
                blade.isLoading = false;
                blade.items = blade.currentEntity.configurationItems.filter(x => x.type === 'Text');
            }

            initialize();
        }]);
