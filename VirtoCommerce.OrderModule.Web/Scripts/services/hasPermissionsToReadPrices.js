angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.hasPermissionsToReadPrices', ['platformWebApp.authService', 'virtoCommerce.orderModule.knownLimitResponseScopes',
        function (authService, knownLimitResponseScopes) {
            return {
                check: function () {
                    var limitResponseScope = 'order:read:OrderLimitResponseScope';
                    var limitResponseScopes = _.map(knownLimitResponseScopes.items, function (item) { return item.id; });
                    var isSetOrderLimitResponseScope = authService.checkPermission(limitResponseScope, limitResponseScopes);
                    var hasPermissionReadOrderPrices = authService.checkPermission(limitResponseScope + ':WithPrices');

                    return hasPermissionReadOrderPrices || !isSetOrderLimitResponseScope;
                }
            };
        }]
    )
    // filter to hide the price if there is no corresponding role
    .filter('verifyPrice', ['virtoCommerce.orderModule.hasPermissionsToReadPrices', function (hasPermissionsToReadPrices) {
        return function (value) {
            return hasPermissionsToReadPrices.check() ? value : value.replace(/\d/g, '#');
        };
    }]);
