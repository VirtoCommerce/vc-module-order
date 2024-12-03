angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.itemConfigurationWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        $scope.openBlade = function () {
            var newBlade = {
                id: "itemConfiguration",
                controller: 'virtoCommerce.orderModule.itemConfigurationDetailController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/item-configuration-detail.tpl.html',
                currentEntity: blade.currentEntity,
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
