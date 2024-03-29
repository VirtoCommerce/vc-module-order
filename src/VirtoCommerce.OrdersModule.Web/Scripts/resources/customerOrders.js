angular.module('virtoCommerce.orderModule')
.factory('virtoCommerce.orderModule.order_res_customerOrders', ['$resource', function ($resource) {
    return $resource('api/order/customerOrders/:id', { id: '@Id' }, {
        search: { method: 'POST', url: 'api/order/customerOrders/search' },
        getNewShipment: { url: 'api/order/customerOrders/:id/shipments/new' },
        getNewPayment: { url: 'api/order/customerOrders/:id/payments/new' },
        recalculate: { method: 'PUT', url: 'api/order/customerOrders/recalculate' },
        update: { method: 'PUT', url: 'api/order/customerOrders' },
        getDashboardStatistics: { url: 'api/order/dashboardStatistics' },
        getOrderChanges: { method: 'GET', url: 'api/order/customerOrders/:id/changes', isArray: true },
        searchOrderChanges: { method: 'POST', url: 'api/order/customerOrders/searchChanges' },
        indexedSearch: { method: 'POST', url: 'api/order/customerOrders/indexed/search' },
        indexedSearchEnabled: { method: 'GET', url: '/api/order/customerOrders/indexed/searchEnabled' },
        capturePayment: { method: 'POST', url: 'api/order/payments/payment/capture' },
        refundPayment: { method: 'POST', url: 'api/order/payments/payment/refund' }
    });
}]);
