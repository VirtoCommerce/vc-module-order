angular.module('virtoCommerce.orderModule')
    // filter to mask the price if argument is false
    .filter('showPrice', function () {
        return function (value, showPrice) {
            var regexp = /\d/g;

            if (regexp.test(value) && arguments.length > 1 && !showPrice) {
                value = String(value).replace(regexp, '#');
            }

            return value;
        };
    });
