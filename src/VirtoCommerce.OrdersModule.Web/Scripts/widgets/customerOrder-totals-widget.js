angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderTotalsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.widget.blade;

            $scope.$watch('widget.blade.customerOrder', function (operation) {
                $scope.operation = operation;
            });

            $scope.openItemsBlade = function () {
                var newBlade = {
                    id: 'customerOrderItems',
                    currentEntity: $scope.operation,
                    recalculateFn: blade.recalculate,
                    parentRefresh: blade.refresh,
                    controller: 'virtoCommerce.orderModule.customerOrderItemsController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-items.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };
}]);
