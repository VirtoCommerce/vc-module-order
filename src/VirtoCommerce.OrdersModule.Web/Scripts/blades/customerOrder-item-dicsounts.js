angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemDiscountController', [
        '$scope', 'platformWebApp.uiGridHelper',
        function ($scope, uiGridHelper) {
            var blade = $scope.blade;
            blade.title = 'orders.blades.customerOrder-item-discounts.title';
            blade.headIcon = 'fa fa-area-chart';
            blade.isVisiblePrices = blade.order && blade.order.withPrices;

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                });
            };

            function initialize() {
                blade.isLoading = false;
            }

            initialize();
        }]);
