angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.captureDetailController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'platformWebApp.authService',
        'virtoCommerce.paymentModule.paymentMethods',
        'virtoCommerce.customerModule.members',
        function ($scope, bladeNavigationService, authService, paymentMethods, members) {
        var blade = $scope.blade;
        blade.isVisiblePrices = authService.checkPermission('order:read_prices');

        blade.title = 'orders.blades.capture-details.title';
        blade.titleValues = { number: blade.currentEntity.number };
        blade.subtitle = 'orders.blades.capture-details.subtitle';

        blade.realOperationsCollection = _.flatten(_.pluck(blade.customerOrder.inPayments, 'captures'));

        blade.remove = function (capture) {
            var payment = _.findWhere(blade.customerOrder.inPayments, { id: capture.paymentId });
            if (payment) {
                var index = _.findIndex(payment.captures, function (x) {
                    return x.id === capture.id;
                });
                if (index >= 0) {
                    payment.captures.splice(index, 1);
                }
            }
        }

        paymentMethods.search({storeId: blade.customerOrder.storeId}, function (data) {
                blade.paymentMethods = data.results;
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
        });

        blade.customInitialize = function () {
            blade.isLocked = false;
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
    }]);
