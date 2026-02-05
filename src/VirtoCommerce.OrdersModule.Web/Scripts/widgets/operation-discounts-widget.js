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
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };
        }
    ]);


