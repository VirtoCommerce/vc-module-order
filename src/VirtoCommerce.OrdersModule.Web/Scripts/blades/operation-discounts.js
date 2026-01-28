angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.operationDiscountsController', [
        '$scope',
        'platformWebApp.uiGridHelper',
        function ($scope, uiGridHelper) {
            var blade = $scope.blade;

            blade.title = 'orders.blades.customerOrder-item-discounts.title';
            blade.headIcon = 'fa fa-area-chart';

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                });
            };

            blade.isLoading = false;
        }
    ]);


