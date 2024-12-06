angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemDiscountWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        $scope.openBlade = function () {
            var newBlade = {
                id: "itemDiscounts",
                controller: 'virtoCommerce.orderModule.customerOrderItemDiscountController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-dicsounts.tpl.html',
                currentEntity: blade.currentEntity,
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
