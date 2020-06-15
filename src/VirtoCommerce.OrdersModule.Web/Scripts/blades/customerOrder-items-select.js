angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.orderItemSelectController', ['$scope', 'platformWebApp.bladeUtils',
    function ($scope, bladeUtils) {
    var blade = $scope.blade;
    var bladeNavigationService = bladeUtils.bladeNavigationService;

    if (!blade.title) {
        blade.title = "Select Order items...";
    }

    $scope.options = angular.extend({
        showCheckingMultiple: true,
        allowCheckingItem: true,
        selectedItemIds: [],
        gridColumns: []
    }, blade.options);

    blade.refresh = function () {
        $scope.items = angular.copy(blade.orderItems);
        _.each($scope.items, function (item) {
            item.quantityOnShipment = item.quantity;
        });
        
        if ($scope.options.onItemsLoaded) {
            $scope.options.onItemsLoaded($scope.items);
        }

        blade.isLoading = false;
    };

    blade.toolbarCommands = [
        {
            name: "orders.commands.add-item", icon: 'fa fa-plus',
            executeMethod: function (blade) {
                var selectedItems = _.map(_.where($scope.items, {selected: true}), function(item) {
                    return { lineItemId: item.id, lineItem: item, quantity: item.quantityOnShipment };
                });
                _.each(selectedItems, function (item) {
                    blade.currentEntity.items.push(item);
                });
                
                bladeNavigationService.closeBlade(blade);
            },
            canExecuteMethod: function () {
                return _.any($scope.items, function (x) { return x.selected; });
            },
            permission: blade.updatePermission
        }
    ];

    blade.refresh();
    
}]);
