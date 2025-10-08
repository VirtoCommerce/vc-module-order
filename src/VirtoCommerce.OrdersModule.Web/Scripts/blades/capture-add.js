angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.captureAddController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.order_res_customerOrders',
    function ($scope, bladeNavigationService, customerOrders) {
        var blade = $scope.blade;

        var operationsCount = blade.payment.childrenOperations ? (blade.payment.childrenOperations.length + 1) : 1;
        var newTransactionId = `${blade.payment.number}-${operationsCount.toString().padStart(3, '0')}`;

        blade.title = 'orders.blades.capture-add.title';
        blade.subtitle = newTransactionId;


        blade.initialize = function () {
            blade.currentEntity = {
                amount: blade.payment.sum,
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

            customerOrders.capturePayment({
                paymentId: blade.payment.id,
                amount: blade.currentEntity.amount,
                captureDetails: blade.currentEntity.captureDetails,
                transactionId: blade.currentEntity.transactionId,
                outerId: blade.currentEntity.outerId,
                closeTransaction: blade.currentEntity.closeTransaction
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

        blade.initialize();
    }]);
