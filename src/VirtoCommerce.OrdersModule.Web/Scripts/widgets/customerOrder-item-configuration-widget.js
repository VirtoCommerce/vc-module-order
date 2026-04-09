angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemConfigurationWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        $scope.openBlade = function () {
            var newBlade = {
                id: "itemConfiguration",
                controller: 'virtoCommerce.orderModule.customerOrderItemConfigurationController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-configuration.tpl.html',
                currentEntity: blade.currentEntity,
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
