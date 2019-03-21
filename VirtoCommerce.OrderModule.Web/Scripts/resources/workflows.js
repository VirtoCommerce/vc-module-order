angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.workflows', ['$resource', function ($resource) {
        return $resource('api/orderworkflow/:id', {},
        {
            updateWorkflow: { method: 'POST', url: 'api/orderworkflow/' }
        });
}]);
