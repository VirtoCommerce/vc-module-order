angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.customerOrderItemsWidgetController', ['$scope', '$translate', 'platformWebApp.bladeNavigationService',
    function ($scope, $translate, bladeNavigationService) {
    var blade = $scope.widget.blade;
    
    $scope.$watch('widget.blade.currentEntity', function (operation) {
        $scope.operation = operation;
    });

    $scope.openItemsBlade = function () {
        $translate('orders.blades.customerOrder-detail.title', { customer: $scope.operation.customerName }).then(function (result) {
            var newBlade = {
                id: 'customerOrderItems',
                title: 'orders.widgets.customerOrder-items.blade-title',
                titleValues: { title: result },
                subtitle: 'orders.widgets.customerOrder-items.blade-subtitle',
                currentEntity: blade.currentEntity,
                recalculateFn: blade.recalculate,
                controller: 'virtoCommerce.orderModule.customerOrderItemsController',
                template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-items.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        });
    };

}]);
