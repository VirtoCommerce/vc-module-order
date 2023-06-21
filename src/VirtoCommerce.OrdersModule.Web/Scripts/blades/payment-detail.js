angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.paymentDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings', 'virtoCommerce.orderModule.order_res_customerOrders', 'virtoCommerce.orderModule.statusTranslationService', 'platformWebApp.authService', 'virtoCommerce.paymentModule.paymentMethods',
    function ($scope, bladeNavigationService, dialogService, settings, customerOrders, statusTranslationService, authService, paymentMethods) {
            var blade = $scope.blade;
            blade.isVisiblePrices = authService.checkPermission('order:read_prices');
            blade.paymentMethods = [];

            blade.isLocked = !blade.currentEntity || (blade.currentEntity.status === 'Paid'
                || blade.currentEntity.cancelledState === 'Requested'
                || blade.currentEntity.cancelledState === 'Completed'
                || blade.currentEntity.isCancelled);
            if (blade.isNew) {
                blade.title = 'orders.blades.payment-detail.title-new';

                var foundField = _.findWhere(blade.metaFields, { name: 'createdDate' });
                if (foundField) {
                    foundField.isReadOnly = false;
                }

                customerOrders.getNewPayment({ id: blade.customerOrder.id }, blade.initialize);
            } else {
                blade.title = 'orders.blades.payment-detail.title';
                blade.titleValues = { number: blade.currentEntity.number };
                blade.subtitle = 'orders.blades.payment-detail.subtitle';
            }

            blade.realOperationsCollection = blade.customerOrder.inPayments;

            paymentMethods.search({ storeId: blade.customerOrder.storeId }, function (data) {
                blade.paymentMethods = data.results;
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });

            settings.getValues({ id: 'PaymentIn.Status' }, translateBladeStatuses);
            blade.openStatusSettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'PaymentIn.Status',
                    parentRefresh: translateBladeStatuses,
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            function translateBladeStatuses(data) {
                blade.statuses = statusTranslationService.translateStatuses(data, 'PaymentIn');
            }

            blade.setEntityStatus = function (status) {
                blade.currentEntity.status = status;
                blade.currentEntity.paymentStatus = status;
            };

            blade.updateRecalculationFlag = function () {
                blade.isTotalsRecalculationNeeded = blade.origEntity.price != blade.currentEntity.price || blade.origEntity.priceWithTax != blade.currentEntity.priceWithTax;
            }

            $scope.$watch("blade.currentEntity.paymentMethod", function (paymentMethod) {
                if (blade.isNew && paymentMethod) {
                    blade.currentEntity.gatewayCode = paymentMethod.code;
                }
            }, true);

            $scope.$watch("blade.currentEntity.documentLoaded", function () {
                console.log('get in payment detail');
                blade.customInitialize();
            }, true);

            blade.customInitialize = function () {
                if (!blade.currentEntity) {
					return;
				}
                blade.isLocked = blade.currentEntity.status === 'Paid' || blade.currentEntity.cancelledState === 'Completed' || blade.currentEntity.isCancelled;
            };

            blade.customInitialize();

        }]);
