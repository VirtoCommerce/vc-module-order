angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderChangeLogController', ['$scope', 'virtoCommerce.orderModule.order_res_customerOrders', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', '$timeout', function ($scope, customerOrders, uiGridHelper, bladeUtils, $timeout) {
        var blade = $scope.blade;

        //pagination settings
        $scope.pageSettings = {};
        $scope.pageSettings.currentPage = 1;
        $scope.pageSettings.itemsPerPageCount = 20;
        $scope.pageSettings.totalItems = 0;

        blade.refresh = function () {
            blade.isLoading = true;

            if ($scope.pageSettings.currentPage !== 1) {
                $scope.pageSettings.currentPage = 1;
            }

            customerOrders.searchOrderChanges(getSearchCriteria(), (data) => {
                blade.isLoading = false;
                $scope.pageSettings.totalItems = data.totalCount;
                blade.currentEntities = data.results;
                $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;

                if ($scope.gridApi) {
                    $scope.gridApi.infiniteScroll.resetScroll(true, true);
                    $scope.gridApi.infiniteScroll.dataLoaded();
                }
            });
        };

        function showMore() {
            if ($scope.hasMore) {
                ++$scope.pageSettings.currentPage;
                $scope.gridApi.infiniteScroll.saveScrollPercentage();
                blade.isLoading = true;

                customerOrders.searchOrderChanges(getSearchCriteria(), (data) => {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    blade.currentEntities = blade.currentEntities.concat(data.results);
                    $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
                    $scope.gridApi.infiniteScroll.dataLoaded();
                });
            }
        }

        function getSearchCriteria() {
            return {
                orderId: blade.orderId,
                sort: uiGridHelper.getSortExpression($scope),
                skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                take: $scope.pageSettings.itemsPerPageCount
            };
        }

        var filter = blade.filter = $scope.filter = {};
        filter.criteriaChanged = function () {
            if ($scope.pageSettings.currentPage > 1) {
                $scope.pageSettings.currentPage = 1;
            } else {
                blade.refresh();
            }
        };

        // ui-grid
        $scope.setGridOptions = function (gridOptions) {
            bladeUtils.initializePagination($scope, true);

            uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                //update gridApi for current grid
                $scope.gridApi = gridApi;
                uiGridHelper.bindRefreshOnSortChanged($scope);
                $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);

            });

            $timeout(function () { blade.refresh(); });
        };

}]);
