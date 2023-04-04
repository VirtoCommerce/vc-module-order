angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.refundAddController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.order_res_customerOrders',
        'virtoCommerce.orderModule.refundReasonsService',
    function ($scope, bladeNavigationService, customerOrders, refundReasonsService) {
        var blade = $scope.blade;
        blade.title = 'orders.blades.refund-add.title';
        blade.subtitle = blade.payment.number;

        blade.initialize = function () {
            blade.currentEntity = {
                amout: blade.payment.sum,
                reasonCode: refundReasonsService.refundCodes[0]
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
            return refundReasonsService.getRefundReasons();
        }

        blade.initialize();
    }]);
