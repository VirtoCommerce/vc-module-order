angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.prices', ['$resource', function ($resource) {
        return $resource('api/products/:id/prices', { id: '@Id', catalogId: '@catalogId' }, {
            search: { url: 'api/catalog/products/prices/search' },
            getProductPrices: { isArray: true }
        });
    }]);
