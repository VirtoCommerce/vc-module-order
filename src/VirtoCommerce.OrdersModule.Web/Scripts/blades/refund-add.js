angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.refundAddController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.order_res_customerOrders',
        'virtoCommerce.orderModule.refundReasonsService',
    function ($scope, bladeNavigationService, customerOrders, refundReasonsService) {
        var blade = $scope.blade;

        var operationsCount = blade.payment.childrenOperations ? (blade.payment.childrenOperations.length + 1) : 1;
        var newTransactionId = `${blade.payment.number}-${operationsCount.toString().padStart(3, '0')}`;

        blade.title = 'orders.blades.refund-add.title';
        blade.subtitle = newTransactionId;

        blade.initialize = function () {
            blade.currentEntity = {
                amount: blade.payment.sum,
                reasonCode: refundReasonsService.refundCodes[0],
                transactionId: newTransactionId
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
                transactionId: blade.currentEntity.transactionId,
                outerId: blade.currentEntity.outerId
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
            return refundReasonsService.getRefundReasons();
        }

        blade.initialize();
    }]);
