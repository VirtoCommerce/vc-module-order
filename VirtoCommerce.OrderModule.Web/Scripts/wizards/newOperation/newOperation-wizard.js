angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.newOperationWizardController', ['$scope', 'virtoCommerce.orderModule.knownOperations', function ($scope, knownOperations) {
    var blade = $scope.blade;
    
    $scope.availableOperations = _.map(blade.availableTypes, function (type) {
        return knownOperations.getOperation(type).newInstanceMetadata;
    });

    blade.isLoading = false;
}]);
