angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.workflowWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.workflows',
            function ($scope, bladeNavigationService, workflows) {
                var blade = $scope.blade;
                var _workflow;
                $scope.workflow = "0";

                workflows.get({ id: blade.currentEntity.id })
                    .$promise
                    .then(function (workflow) {
                        workflow = workflow.data;
                        $scope.workflow = typeof workflow !== 'undefined' && typeof workflow.workflowName !== 'undefined' && workflow.workflowName.length ? "1" : "0";
                        _workflow = workflow;
                    }, function(response) {
                        bladeNavigationService.setError(response, blade);
                    });

                $scope.openWorkflowBlade = function () {
                    var newBlade = {
                        id: 'workflowBlade',
                        title: 'orders.blades.organization-workflow.title',
                        controller: 'virtoCommerce.orderModule.uploadWorkflowController',
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/upload-workflow.tpl.html',
                        isClosingDisabled: true,
                        currentEntity: blade.currentEntity,
                        currentEntityId: blade.currentEntityId
                    };
                    if (_workflow) newBlade = angular.extend({}, newBlade, { workflow: _workflow });
                    bladeNavigationService.showBlade(newBlade, $scope.blade);
                };
}]);
