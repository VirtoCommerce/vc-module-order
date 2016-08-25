angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.operationTreeWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.$watch('widget.blade.customerOrder', function (order) {
        if (!$scope.currentOperationId) {
            $scope.currentOperationId = order.id;
        }

        var treeRoot = {};
        buildOperationsTree(order, treeRoot)
        $scope.node = treeRoot.childrenNodes[0];

    });

    function buildOperationsTree(op, tree) {
        var foundTemplate = _.findWhere(OrderModule_knownOperations, { type: op.operationType }) || {};
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
        if (node.getDetailBlade) {
            var newBlade = node.getDetailBlade(node.operation, blade);
            if (newBlade)
                bladeNavigationService.showBlade(newBlade, blade);
        }
    };

}]);
