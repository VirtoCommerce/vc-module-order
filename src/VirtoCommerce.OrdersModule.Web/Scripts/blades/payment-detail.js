angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.paymentDetailController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.orderModule.order_res_customerOrders',
        'platformWebApp.authService',
        'virtoCommerce.paymentModule.paymentMethods',
        'virtoCommerce.customerModule.members',
        'virtoCommerce.orderModule.knownOperations',
        function ($scope, bladeNavigationService, customerOrders, authService, paymentMethods, members, knownOperations) {
            var blade = $scope.blade;
            blade.isVisiblePrices = authService.checkPermission('order:read_prices');
            blade.paymentMethods = [];

            blade.captureStatuses = ['Authorized', 'Paid'];
            blade.refundStatuses = ['PartiallyRefunded', 'Paid'];
            blade.capturePermission = 'order:capture_payment';
            blade.refundPermission = 'order:refund';

            if (blade.isNew) {
                blade.title = 'orders.blades.payment-detail.title-new';

                var foundField = _.findWhere(blade.metaFields, { name: 'createdDate' });
                if (foundField) {
                    foundField.isReadOnly = false;
                }

                customerOrders.getNewPayment({ id: blade.customerOrder.id },
                    function (data) {
                        data.createdDate = null;
                        blade.initialize(data);
                    },
                    function (error) {
                        blade.isLocked = true;
                        bladeNavigationService.setError(error.data, blade);
                });
            } else {
                blade.isLocked = knownOperations.isLocked("PaymentIn", blade.currentEntity);

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

            blade.toolbarCommands.push({
                name: 'orders.blades.payment-detail.labels.capture-payment',
                icon: 'fas fa-file-text',
                index: 2,
                permission: blade.capturePermission,
                executeMethod: function () {
                    var newBlade = {
                        id: 'capture-add',
                        title: 'orders.blades.capture-add.title',
                        payment: blade.currentEntity,
                        parentRefresh: function () {
                            blade.refresh();
                            blade.parentBlade.refresh();
                        },
                        controller: 'virtoCommerce.orderModule.captureAddController',
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/capture-add.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                },
                canExecuteMethod: function () {
                    return _.find(blade.captureStatuses, function (x) {
                        return !blade.currentEntity || x === blade.currentEntity.status
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
                        return !blade.currentEntity || x === blade.currentEntity.status
                    });
                }
            });

            blade.setEntityStatus = function (status) {
                blade.currentEntity.status = status;
                blade.currentEntity.paymentStatus = status;
            };

            blade.updateRecalculationFlag = function () {
                blade.isTotalsRecalculationNeeded = blade.origEntity.price != blade.currentEntity.price || blade.origEntity.priceWithTax != blade.currentEntity.priceWithTax;
            }

            blade.fetchVendors = function (criteria) {
                if (blade.isLocked) {
                    return [];
                }

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

            $scope.$watch("blade.currentEntity.documentLoaded", function () {
                console.log('get in payment detail');
                blade.customInitialize();
            }, true);

            blade.customInitialize = function () {
                if (!blade.currentEntity) {
                    return;
                }
                blade.isLocked = knownOperations.isLocked("PaymentIn", blade.currentEntity);
            };

            blade.customInitialize();

        }]);
