angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemDiscountWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        $scope.openBlade = function () {
            var newBlade = {
                id: "itemDiscounts",
                controller: 'virtoCommerce.orderModule.customerOrderItemDiscountController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-dicsounts.tpl.html',
                currentEntity: blade.currentEntity,
                // A line item carries no withPrices flag of its own, so the order's is passed down
                order: blade.order,
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
