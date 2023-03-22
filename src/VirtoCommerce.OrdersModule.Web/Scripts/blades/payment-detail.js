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
        'currencyFilter',
        function ($scope, bladeNavigationService, dialogService, settings, customerOrders, statusTranslationService, authService, paymentMethods, members, currencyFilter) {
        var blade = $scope.blade;
        blade.isVisiblePrices = authService.checkPermission('order:read_prices');
        blade.paymentMethods = [];

        blade.captureStatuses = ['Authorized'];
        blade.refundStatuses = ['PartiallyRefunded', 'Paid'];
        blade.capturePermission = 'order:capture_payment';
        blade.refundPermission = 'order:refund';

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
            permission: blade.capturePermission,
            executeMethod: function () {
                var amount = currencyFilter(blade.currentEntity.sum, blade.currentEntity.currency).replace(/\s/g, '');

                var dialog = {
                    id: "confirmCapture",
                    title: "orders.dialogs.payment-capture.title",
                    message: "orders.dialogs.payment-capture.message",
                    messageValues: { amount: amount },
                    callback: function (capture) {
                        if (capture) {
                            blade.isLoading = true;

                            customerOrders.capturePayment({
                                paymentId: blade.currentEntity.id
                            }, function (data) {
                                blade.isLoading = false;

                                if (data.succeeded) {
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
                        }
                    }
                };

                dialogService.showConfirmationDialog(dialog);
            },
            canExecuteMethod: function () {
                return _.find(blade.captureStatuses, function (x) {
                    return x === blade.currentEntity.status
                });
            }
        });

        blade.toolbarCommands.push({
            name: 'orders.blades.payment-detail.labels.refund-payment',
            icon: 'fas fa-file-text',
            index: 2,
            permission: blade.refundPermission,
            executeMethod: function () {
                var newBlade = {
                    id: 'refund-add',
                    title: 'orders.blades.refund-add.title',
                    payment: blade.currentEntity,
                    parentRefresh: function () {
                        blade.refresh();
                        blade.parentBlade.refresh();
                    },
                    controller: 'virtoCommerce.orderModule.refundAddController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/refund-add.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            },
            canExecuteMethod: function () {
                return _.find(blade.refundStatuses, function (x) {
                    return x === blade.currentEntity.status
                });
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
