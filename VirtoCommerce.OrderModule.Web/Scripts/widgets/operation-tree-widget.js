angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.operationTreeWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.knownOperations', 'virtoCommerce.orderModule.hasPermissionsToReadPrices', function ($scope, bladeNavigationService, knownOperations, hasPermissionsToReadPrices) {
    var blade = $scope.blade;

    $scope.checkVisiblePrices = hasPermissionsToReadPrices.check;

    $scope.$watchCollection('blade.customerOrder.childrenOperations', function () {
        $scope.currentOperationId = blade.customerOrder.id;

        if (!blade.isLoading) {
            var treeRoot = {};
            buildOperationsTree(blade.customerOrder, treeRoot)
            $scope.node = treeRoot.childrenNodes[0];
        }
    });

    function buildOperationsTree(op, tree) {
        var foundTemplate = knownOperations.getOperation(op.operationType);
        var nodeTemplate = angular.copy(foundTemplate);
        nodeTemplate.operation = op;
        tree.childrenNodes = tree.childrenNodes || [];
        tree.childrenNodes.push(nodeTemplate);
        _.each(op.childrenOperations, function (o) {
            buildOperationsTree(o, nodeTemplate);
        });
    }

    $scope.selectOperation = function (node) {
        $scope.currentOperationId = node.operation.id;

        if (node.operation.id === blade.customerOrder.id) {
            bladeNavigationService.closeChildrenBlades(blade);
        } else {
            var newBlade = node.detailBlade;
            angular.extend(newBlade, {
                customerOrder: blade.customerOrder,
                currentEntity: node.operation
            });

            bladeNavigationService.showBlade(newBlade, blade);
        }
    };

}]);
