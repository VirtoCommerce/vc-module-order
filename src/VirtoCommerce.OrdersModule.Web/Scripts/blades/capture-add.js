angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.captureAddController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.order_res_customerOrders',
    function ($scope, bladeNavigationService, customerOrders) {
        var blade = $scope.blade;
        blade.title = 'orders.blades.refund-add.title';
        blade.subtitle = blade.payment.number;

        blade.initialize = function () {
            blade.currentEntity = {
                amount: blade.payment.sum,
                transactionId: blade.payment.number
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

            customerOrders.capturePayment({
                paymentId: blade.payment.id,
                amount: blade.currentEntity.amount,
                captureDetails: blade.currentEntity.captureDetails,
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
