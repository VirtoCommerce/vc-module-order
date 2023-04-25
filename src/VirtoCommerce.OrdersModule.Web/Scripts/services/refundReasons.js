angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.refundReasonsService', ['$translate', function ($translate) {
        var refundCodes = ['Duplicate', 'Fraudulent', 'RequestedByCustomer', 'Other'];

        function translateCode(code) {
            var translateKey = 'orders.settings.reason-codes.' + code.toLowerCase();
            var result = $translate.instant(translateKey);
            return result === translateKey ? code : result;
        }

        return {
            refundCodes: refundCodes,
            getRefundReasons: function () {
                return _.map(refundCodes, function (x) {
                    return {
                        id: x,
                        name: translateCode(x)
                    };
                });
            }
        };
    }])

