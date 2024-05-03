angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.operationDetailController', ['$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.order_res_customerOrders', 'platformWebApp.objCompareService', '$timeout', 'focus', '$rootScope',
    function ($scope,dialogService, bladeNavigationService, customerOrders, objCompareService, $timeout, focus, $rootScope) {
        var blade = $scope.blade;
        blade.updatePermission = 'order:update';

        blade.refresh = function () {
            if (blade.id === 'operationDetail') {
                if (!blade.isNew)
                    blade.initialize(blade.currentEntity);
            }
            else {
                blade.isLoading = true;
                customerOrders.get({ id: blade.customerOrder.id }, function (result) {
                    blade.initialize(result);
                    blade.customerOrder = blade.currentEntity;
                    //necessary for scope bounded ACL checks 
                    blade.securityScopes = result.scopes;
                });
            }
        }

        blade.initialize = function (operation) {
            blade.origEntity = operation;
            blade.currentEntity = angular.copy(operation);
            $scope.$broadcast("blade.currentEntity.documentLoaded");
            blade.isLoading = false;
        };

        // base functions to override as needed
        blade.customInitialize = function () { };
        blade.setEntityStatus = function (status) {
            blade.currentEntity.status = status;
        };

        function clearCreatedDateIfEmpty(items) {
            _.forEach(items, function (item) {
                if (item.createdDate === '0001-01-01T00:00:00Z') {
                    item.createdDate = null;
                }
            });
        }

        blade.recalculate = function () {
            blade.isLoading = true;
            customerOrders.recalculate(blade.customerOrder, function (result) {
                clearCreatedDateIfEmpty(result.inPayments);
                clearCreatedDateIfEmpty(result.shipments);
                clearCreatedDateIfEmpty(result.childrenOperations);
                angular.copy(result, blade.customerOrder);

                var idToFocus = document.activeElement.id;
                if (idToFocus)
                    $timeout(function () {
                        focus(idToFocus);
                        document.getElementById(idToFocus).select();
                    });

                blade.isLoading = false;
            });
        };

        function isDirty() {
            return blade.origEntity &&
                !objCompareService.equal(blade.origEntity, blade.currentEntity) &&
                !blade.isNew &&
                !blade.isConcurrencyConflict &&
                blade.hasUpdatePermission();
        }

        function canSave() {
            return isDirty() && (!$scope.formScope || $scope.formScope.$valid);
        }

        $scope.setForm = function (form) { $scope.formScope = form; };

        $scope.cancelChanges = function () {
            blade.currentEntity = blade.origEntity;
            $scope.bladeClose();
        }
        $scope.saveChanges = function () {
            if (blade.id === 'operationDetail') {
                angular.copy(blade.currentEntity, blade.origEntity);
                if (blade.isNew) {
                    blade.realOperationsCollection.push(blade.currentEntity);
                    blade.customerOrder.childrenOperations.push(angular.copy(blade.currentEntity));
                } else {
                    var foundOp = _.findWhere(blade.realOperationsCollection, { id: blade.origEntity.id });
                    angular.copy(blade.origEntity, foundOp);
                }
                $scope.bladeClose();

                if (blade.isTotalsRecalculationNeeded) {
                    blade.recalculate();
                }
            } else {
                blade.isLoading = true;
                customerOrders.update(blade.customerOrder, function () {
                        blade.isNew = false;
                        blade.refresh();
                        blade.parentBlade.refresh();
                        bladeNavigationService.closeChildrenBlades(blade);
                        },
                    function (error) {
                        if (error.status === 409) {
                            const dialog = {
                                title: "orders.dialogs.concurrency-сonflict.title",
                                message: "orders.dialogs.concurrency-сonflict.message",
                                callback: function () {
                                    blade.isNew = false;
                                    blade.isConcurrencyConflict = false;
                                    blade.refresh();
                                    blade.parentBlade.refresh();
                                    bladeNavigationService.closeChildrenBlades(blade);
                                },
                                callbackOnDismiss: function () {
                                    blade.isConcurrencyConflict = true;
                                }
                            }

                            dialogService.showErrorDialog(dialog);
                        }
                    }
                );
            }
        };

        blade.toolbarCommands = [
        {
            name: "orders.commands.new-document", icon: 'fas fa-plus',
            executeMethod: function () {
                var newBlade = {
                    id: "newOperationWizard",
                    customerOrder: blade.customerOrder,
                    currentEntity: blade.currentEntity,
                    availableTypes: blade.knownChildrenOperations,
                    title: "orders.blades.newOperation-wizard.title",
                    subtitle: 'orders.blades.newOperation-wizard.subtitle',
                    controller: 'virtoCommerce.orderModule.newOperationWizardController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/wizards/newOperation/newOperation-wizard.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            },
            canExecuteMethod: function () {
                return _.any(blade.knownChildrenOperations);
            },
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.save", icon: 'fas fa-save',
            executeMethod: $scope.saveChanges,
            canExecuteMethod: canSave,
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.reset", icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origEntity, blade.currentEntity);
            },
            canExecuteMethod: isDirty,
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.delete", icon: 'fas fa-trash-alt',
            executeMethod: function () {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "orders.dialogs.operation-delete.title",
                    message: "orders.dialogs.operation-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            if (blade.id === 'operationDetail') {
                                function removeChildrenOperation(childrenOperations, operationId) {
                                    if (childrenOperations && childrenOperations.length) {
                                        var index = _.findIndex(childrenOperations, function (x) {
                                            return x.id === operationId;
                                        });
                                        if (index >= 0) {
                                            childrenOperations.splice(index, 1);
                                            return;
                                        }
                                        else {
                                            for (var operation of childrenOperations) {
                                                removeChildrenOperation(operation.childrenOperations, operationId);
                                            }
                                        }
                                    }
                                }
                                removeChildrenOperation(blade.customerOrder.childrenOperations, blade.origEntity.id);

                                var idx = _.findIndex(blade.realOperationsCollection, function (x) { return x.id === blade.origEntity.id; });
                                blade.realOperationsCollection.splice(idx, 1);

                                if (blade.currentEntity.operationType === 'Refund' || blade.currentEntity.operationType === 'Capture') {
                                    blade.remove(blade.origEntity);
                                }

                                $rootScope.$broadcast('update-operation-tree');
                                bladeNavigationService.closeBlade(blade);
                            }
                            else {
                                customerOrders.delete({ ids: blade.customerOrder.id }, function () {
                                    blade.parentBlade.refresh();
                                    bladeNavigationService.closeBlade(blade);
                                });
                            }
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            },
            canExecuteMethod: function () {
                return true;
            },
            permission: 'order:delete'
        },
        {
            name: "orders.commands.cancel-document", icon: 'fa fa-remove',
            executeMethod: function () {
                var dialog = {
                    id: "confirmCancelOperation",
                    callback: function (reason) {
                        switch (blade.currentEntity.operationType) {
                            case 'PaymentIn':
                                blade.currentEntity.cancelledState = 'Requested';
                                break;
                            case 'CustomerOrder':
                                _.forEach(blade.currentEntity.inPayments, function (payment) {
                                    payment.cancelledState = 'Requested';
                                });

                                blade.currentEntity.isCancelled = true;
                                blade.setEntityStatus('Cancelled');
                                break;
                            default:
                                blade.currentEntity.isCancelled = true;
                                blade.setEntityStatus('Cancelled');
                        }

                        blade.currentEntity.cancelReason = reason == null || reason.replace(/\s/g, '').length < 1 ? null : reason;
                        blade.currentEntity.cancelledDate = new Date();
                        $scope.saveChanges();
                    }
                };
                dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.Orders)/Scripts/dialogs/cancelOperation-dialog.tpl.html', 'virtoCommerce.orderModule.confirmCancelDialogController');
            },
            canExecuteMethod: function () {
                var result = false;

                if (blade.currentEntity) {
                    switch (blade.currentEntity.operationType) {
                        case 'CustomerOrder':
                            result = !blade.currentEntity.isCancelled && blade.currentEntity.status !== 'Completed';
                            break;
                        case 'PaymentIn':
                            result = !(blade.currentEntity.isCancelled
                                || blade.currentEntity.cancelledState === 'Completed'
                                || blade.currentEntity.cancelledState === 'Requested');
                            break;
                        default:
                            result = !blade.currentEntity.isCancelled;
                            break;
                    }
                }

                return result;
            },
            permission: blade.updatePermission
        }
        ];

        // no save for children operations
        if (blade.id === 'operationDetail') {
            blade.toolbarCommands.splice(1, 1);
        }

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "orders.dialogs.operation-save.title", "orders.dialogs.operation-save.message");
        };

        $scope.cancelOperationResolution = function (resolution) {
            $modalInstance.close(resolution);
        };

        // actions on load
        blade.refresh();
    }
])
.controller('virtoCommerce.orderModule.confirmCancelDialogController', ['$scope', '$modalInstance', function ($scope, $modalInstance, dialog) {

    $scope.cancelReason = undefined;
    $scope.yes = function () {
        $modalInstance.close($scope.cancelReason);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
}]);
