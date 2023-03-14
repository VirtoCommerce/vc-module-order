angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.refundAddController', [
        '$scope',
        '$translate',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.order_res_customerOrders',
    function ($scope, $translate, bladeNavigationService, customerOrders) {
        var blade = $scope.blade;
        blade.title = 'orders.blades.refund-add.title';
        blade.subtitle = blade.payment.number;

        blade.refundCodes = ['Duplicate', 'Fraudulent', 'RequestedByCustomer', 'Other'];

        blade.initialize = function () {
            blade.currentEntity = {
                amout: blade.payment.sum,
                reasonCode: blade.refundCodes[0]
            };

            blade.isLoading = false;
        };

        $scope.setForm = function (form) {
            $scope.formScope = form;
        };

        $scope.cancelChanges = function () {
            $scope.bladeClose();
        }

        $scope.saveChanges = function () {
            blade.isLoading = true;

            customerOrders.refundPayment({
                paymentId: blade.payment.id,
                amount: blade.currentEntity.amount,
                reasonCode: blade.currentEntity.reasonCode,
                reasonMessage: blade.currentEntity.reasonMessage,
            }, function (data) {
                blade.isLoading = false;

                if (data.succeeded) {
                    blade.parentRefresh();
                    $scope.bladeClose();
                }
                else {
                    bladeNavigationService.setError(data.errorMessage, blade);
                }
            }, function (error) {
                blade.isLoading = false;
                bladeNavigationService.setError('Error ' + error.status, blade);
            })
        };

        $scope.getRefundReasons = function () {
            return _.map(blade.refundCodes, function (x) {
                return {
                    id: x,
                    name: translateCode(x)
                };
            });
        }

        function translateCode(code) {
            var translateKey = 'orders.blades.refund-add.reason-codes.' + code.toLowerCase();
            var result = $translate.instant(translateKey);
            return result === translateKey ? code : result;
        }

        blade.initialize();
    }]);
