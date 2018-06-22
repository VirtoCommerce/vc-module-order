angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderChangeLogWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.widget.blade;

        blade.showOrderChanges = function () {
            var newBlade = {
                id: 'customerOrderChangeLog',
                orderId: blade.customerOrder.id,
                controller: 'virtoCommerce.orderModule.customerOrderChangeLogController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-change-log.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
