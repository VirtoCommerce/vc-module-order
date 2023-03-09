angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.paymentDetailController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'platformWebApp.dialogService',
        'platformWebApp.settings',
        'virtoCommerce.orderModule.order_res_customerOrders',
        'virtoCommerce.orderModule.statusTranslationService',
        'platformWebApp.authService',
        'virtoCommerce.paymentModule.paymentMethods',
        'virtoCommerce.customerModule.members',
    function ($scope, bladeNavigationService, dialogService, settings, customerOrders, statusTranslationService, authService, paymentMethods, members) {
        var blade = $scope.blade;
        blade.isVisiblePrices = authService.checkPermission('order:read_prices');
        blade.paymentMethods = [];

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

        paymentMethods.search({storeId: blade.customerOrder.storeId}, function (data) {
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

        blade.toolbarCommands.push({
            name: 'orders.blades.payment-detail.labels.capture-payment',
            icon: 'fas fa-file-text',
            index: 1,
            executeMethod: function (blade) {
                blade.isLoading = true;

                customerOrders.capturePayment({
                    paymentId: blade.currentEntity.id
                }, function (data) {
                    blade.isLoading = false;

                    if (data.isSuccess) {
                        blade.currentEntity.status = 'Paid';
                        blade.refresh();
                        blade.parentBlade.refresh();
                    }
                    else {
                        bladeNavigationService.setError(data.errorMessage, blade);
                    }
                }, function (error) {
                    blade.isLoading = false;

                    bladeNavigationService.setError('Error ' + error.status, blade);
                })
            },
            canExecuteMethod: function () {
                return blade.currentEntity.status === 'Authorized';;
            }
        });

        function translateBladeStatuses(data) {
            blade.statuses = statusTranslationService.translateStatuses(data, 'PaymentIn');
        }

        blade.customInitialize = function () {
            blade.isLocked = blade.currentEntity.status == 'Paid' || blade.currentEntity.cancelledState === 'Completed' || blade.currentEntity.isCancelled;
        };

        blade.setEntityStatus = function (status) {
            blade.currentEntity.status = status;
            blade.currentEntity.paymentStatus = status;
        };

        blade.updateRecalculationFlag = function () {
            blade.isTotalsRecalculationNeeded = blade.origEntity.price != blade.currentEntity.price || blade.origEntity.priceWithTax != blade.currentEntity.priceWithTax;
        }

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

        $scope.$watch("blade.currentEntity.paymentMethod", function (paymentMethod) {
            if (blade.isNew && paymentMethod) {
                blade.currentEntity.gatewayCode = paymentMethod.code;
            }
        }, true);
    }]);
