angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.newOperationWizardController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.orderModule.order_res_customerOrders', function ($scope, bladeNavigationService, dialogService, order_res_customerOrders) {
    var blade = $scope.blade;

    $scope.availableOperations = _.pluck(
            _.filter(OrderModule_knownOperations, function (x) {
                return _.any(blade.availableChildrenTypes, function (type) { return type === x.type; })
            }),
        'newInstanceMetadata')

    blade.isLoading = false;
}]);
