angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.hasPermissionsToReadPrices', ['platformWebApp.authService', 'virtoCommerce.orderModule.knownLimitResponseScopes',
        function (authService, knownLimitResponseScopes) {
            var cachedPermissionValue = null;

            return {
                check: function () {
                    if (cachedPermissionValue !== null) {
                        return cachedPermissionValue;
                    }

                    var limitResponseScope = 'order:read:OrderLimitResponseScope';
                    var limitResponseScopes = _.map(knownLimitResponseScopes.items, function (item) { return item.id; });
                    var isSetOrderLimitResponseScope = authService.checkPermission(limitResponseScope, limitResponseScopes);
                    var hasPermissionReadOrderPrices = authService.checkPermission(limitResponseScope + ':WithPrices');

                    cachedPermissionValue = hasPermissionReadOrderPrices || !isSetOrderLimitResponseScope;

                    return cachedPermissionValue;
                },

                reCheck: function () {
                    cachedPermissionValue = null;
                    return this.check();
                }
            };
        }]
    )
    // filter to hide the price if there is no corresponding role
    .filter('verifyPrice', ['virtoCommerce.orderModule.hasPermissionsToReadPrices', function (hasPermissionsToReadPrices) {
        return function (value) {
            return hasPermissionsToReadPrices.check() ? value : value.replace(/\d/g, '*');
        };
    }]);
