angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.itemConfigurationDetailController', [
        '$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService',
        function ($scope, uiGridHelper, bladeNavigationService) {
            var blade = $scope.blade;
            blade.title = 'orders.blades.configuration.title';
            blade.headIcon = 'fas fa-sliders';

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                });
            };

            $scope.selectNode = function (item) {
                $scope.selectedNodeId = item.id;

                var newBlade = {
                    id: "listItemDetail",
                    controller: 'virtoCommerce.catalogModule.itemDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html',
                    title: item.name,
                    itemId: item.productId
                };
                bladeNavigationService.showBlade(newBlade, blade);
            }

            function initialize() {
                blade.isLoading = false;
            }

            initialize();
        }]);
