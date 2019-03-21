angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.workflows', ['$resource', function ($resource) {
        return $resource('api/workflows/:id', {},
        {
            updateWorkflow: { method: 'POST', url: 'api/workflows/' }
        });
}]);
