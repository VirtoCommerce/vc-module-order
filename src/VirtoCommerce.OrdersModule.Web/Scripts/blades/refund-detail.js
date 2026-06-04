angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.refundDetailController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'platformWebApp.authService',
        'virtoCommerce.paymentModule.paymentMethods',
        'virtoCommerce.customerModule.members',
        'virtoCommerce.orderModule.refundReasonsService',
        function ($scope, bladeNavigationService, authService, paymentMethods, members, refundReasonsService) {
            var blade = $scope.blade;
            blade.isVisiblePrices = blade.currentEntity.withPrices;

            blade.title = 'orders.blades.refund-details.title';
            blade.titleValues = { number: blade.currentEntity.number };
            blade.subtitle = 'orders.blades.refund-details.subtitle';

            blade.realOperationsCollection = _.flatten(_.pluck(blade.customerOrder.inPayments, 'refunds'));

            blade.remove = function (refund) {
                var payment = _.findWhere(blade.customerOrder.inPayments, { id: refund.paymentId });
                if (payment) {
                    var index = _.findIndex(payment.refunds, function (x) {
                        return x.id === refund.id;
                    });
                    if (index >= 0) {
                        payment.refunds.splice(index, 1);
                    }
                }
            }

            paymentMethods.search({ storeId: blade.customerOrder.storeId }, function (data) {
                blade.paymentMethods = data.results;
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });

            blade.setEntityStatus = function (status) {
                blade.currentEntity.status = status;
            };

            blade.fetchVendors = function (criteria) {
                return members.search(criteria);
            }

            blade.openVendorsManagement = function () {
                var newBlade = {
                    id: 'vendorList',
                    currentEntity: { id: null },
                    controller: 'virtoCommerce.customerModule.memberListController',
                    template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.getRefundReasons = function () {
                return refundReasonsService.getRefundReasons();
            }

            $scope.$watch("blade.currentEntity.documentLoaded", function () {
                blade.customInitialize();
            }, true);

            blade.customInitialize = function () {
                blade.isLocked = false;
            };

            blade.customInitialize();
        }]);
