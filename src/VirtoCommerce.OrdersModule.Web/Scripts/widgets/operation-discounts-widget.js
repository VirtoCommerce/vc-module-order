angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.operationDiscountWidgetController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.blade;

            $scope.openBlade = function () {
                var newBlade = {
                    id: "operationDiscounts",
                    controller: 'virtoCommerce.orderModule.operationDiscountsController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/operation-discounts.tpl.html',
                    currentEntity: blade.currentEntity,
                    // Taken from the host blade rather than from currentEntity: this widget also sits on
                    // the line item blade, and a LineItem carries no withPrices flag of its own.
                    isVisiblePrices: blade.isVisiblePrices,
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };
        }
    ]);


