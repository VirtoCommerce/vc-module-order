angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemConfigurationFilesController', [
        '$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService',
        function ($scope, uiGridHelper, bladeNavigationService) {
            var blade = $scope.blade;
            blade.title = 'orders.blades.customerOrder-item-configuration.menu.files.title';
            blade.headIcon = 'fa fa-file';

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
                var itemFiles = blade.currentEntity.configurationItems.filter(x => x.type === 'File').map(x => x.files);
                blade.items = [].concat.apply([], itemFiles);
            }

            initialize();
        }]);
