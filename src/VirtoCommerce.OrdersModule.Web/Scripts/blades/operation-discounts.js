angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.operationDiscountsController', [
        '$scope',
        'platformWebApp.uiGridHelper',
        function ($scope, uiGridHelper) {
            var blade = $scope.blade;

            blade.title = 'orders.blades.customerOrder-item-discounts.title';
            blade.headIcon = 'fa fa-area-chart';
            // Normally supplied by the widget from the host blade. Fall back to the entity's own flag
            // for callers that open this blade directly with an order, payment or shipment.
            if (blade.isVisiblePrices === undefined) {
                blade.isVisiblePrices = blade.currentEntity && blade.currentEntity.withPrices;
            }

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                });
            };

            blade.isLoading = false;
        }
    ]);


