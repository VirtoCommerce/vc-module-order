angular.module('virtoCommerce.orderModule')
    .factory('virtoCommerce.orderModule.statusTranslationService', ['$translate', function ($translate) {
        return {
            translateStatuses: function (rawStatuses, operationType) {
                return _.map(rawStatuses, function (x) {
                    return {
                        key: x,
                        value: translateOrderStatus(x, operationType, $translate)
                    };
                });
            }
        };
    }])
    // operation statuses localization filter
    .filter('statusTranslate', ['$translate', 'settingTranslateFilter', function ($translate, settingTranslateFilter) {
        return function (statusOrOperation, operation) {
            operation = operation || statusOrOperation;

            if (!operation || !operation.status) {
                return '';
            }

            const status = operation.status;
            const operationType = operation.operationType;

            if (!operationType) {
                return status;
            }

            switch (operationType.toLowerCase()) {
                case 'customerorder':
                    return settingTranslateFilter(status, 'Order.Status')
                case 'line-item':
                    return settingTranslateFilter(status, 'OrderLineItem.Statuses')
                case 'shipment':
                    return settingTranslateFilter(status, 'Shipment.Status')
                case 'paymentin':
                    return settingTranslateFilter(status, 'PaymentIn.Status')
                case 'refund':
                    return settingTranslateFilter(status, 'Refund.Status')
                default:
                    return translateOrderStatus(status, operationType, $translate);
            }
        };
    }])
    ;

function translateOrderStatus(rawStatus, operationType, $translate) {
    var translateKey = 'orders.settings.' + operationType.toLowerCase() + '-status.' + rawStatus.toLowerCase();
    var result = $translate.instant(translateKey);
    return result === translateKey ? rawStatus : result;
}
