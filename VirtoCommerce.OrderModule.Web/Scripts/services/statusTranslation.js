angular.module('virtoCommerce.orderModule')
.factory('virtoCommerce.orderModule.statusTranslationService', ['$translate', function ($translate) {
    return {
        translateStatuses: function (rawStatuses, operationType) {
            var translatePrefix = 'orders.settings.' + operationType.toLowerCase() + '-status.';
            return _.map(rawStatuses, function (x) {
                var translateKey = translatePrefix + x.toLowerCase();
                var result = $translate.instant(translateKey);
                return {
                    key: x,
                    value: result === translateKey ? x : result
                };
            });
        }
    };
}])
// operation statuses localization filter
.filter('statusTranslate', ['$translate', function ($translate) {
    return function (statusOrOperation, operation) {
        operation = operation || statusOrOperation;
        var translateKey = 'orders.settings.' + operation.operationType.toLowerCase() + '-status.' + operation.status.toLowerCase();
        var result = $translate.instant(translateKey);

        return result === translateKey ? operation.status : result;
    };
}])
;