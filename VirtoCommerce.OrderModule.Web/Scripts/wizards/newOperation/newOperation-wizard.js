angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.newOperationWizardController', ['$scope', function ($scope) {
    var blade = $scope.blade;

    var ops = _.filter(OrderModule_knownOperations, function (x) {
        return blade.availableChildrenTypes.indexOf(x.type) >= 0;
    });
    $scope.availableOperations = _.pluck(ops, 'newInstanceMetadata');

    blade.isLoading = false;
}]);
