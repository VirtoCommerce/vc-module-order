angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderDetailController', ['$scope', '$window', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings', 'virtoCommerce.customerModule.members', 'virtoCommerce.customerModule.memberTypesResolverService', 'virtoCommerce.orderModule.statusTranslationService', 'virtoCommerce.orderModule.securityAccounts', 'platformWebApp.authService',
        function ($scope, $window, bladeNavigationService, dialogService, settings, members, memberTypesResolverService, statusTranslationService, securityAccounts, authService) {
            var blade = $scope.blade;
            blade.currentEntityId = blade.customerOrder.id;

            blade.isVisiblePrices = authService.checkPermission('order:read_prices');

            angular.extend(blade, {
                title: 'orders.blades.customerOrder-detail.title',
                titleValues: { customer: blade.customerOrder.customerName },
                subtitle: 'orders.blades.customerOrder-detail.subtitle'
            });

            blade.toolbarCommands.push({
                name: 'orders.blades.customerOrder-detail.labels.invoice',
                icon: 'fa fa-download',
                index: 5,
                executeMethod: function (blade) {
                    $window.open('api/order/customerOrders/invoice/' + blade.currentEntity.number, '_blank');
                },
                canExecuteMethod: function () {
                    return true;
                }
            });

            settings.getValues({ id: 'Order.Status' }, translateBladeStatuses);
            blade.openStatusSettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Order.Status',
                    parentRefresh: translateBladeStatuses,
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            function translateBladeStatuses(data) {
                blade.statuses = statusTranslationService.translateStatuses(data, 'customerOrder');
            }

            function showCustomerDetailBlade(member) {
                var foundTemplate = memberTypesResolverService.resolve(member.memberType);
                if (foundTemplate) {
                    var newBlade = angular.copy(foundTemplate.detailBlade);
                    newBlade.currentEntity = member;
                    bladeNavigationService.showBlade(newBlade, blade);
                } else {
                    dialogService.showNotificationDialog({
                        id: "error",
                        title: "customer.dialogs.unknown-member-type.title",
                        message: "customer.dialogs.unknown-member-type.message",
                        messageValues: { memberType: member.memberType }
                    });
                }
            }

            blade.openCustomerDetails = function () {
                securityAccounts.get({ id: blade.customerOrder.customerId }, function (account) {
                    var contactId = (account && account.memberId) ? account.memberId : blade.customerOrder.customerId;
                    members.get({ id: contactId }, function (member) {
                        if (member && member.id) {
                            showCustomerDetailBlade(member);
                        }
                    });
                });
            };

            blade.fetchEmployees = function (criteria) {
                criteria.memberType = 'Employee';
                criteria.deepSearch = true;
                criteria.sort = 'name';

                return members.search(criteria);
            };

            blade.customInitialize = function () {
                if (!blade.currentEntity) return;
                blade.isLocked = blade.currentEntity.status === 'Completed' || blade.currentEntity.cancelledState === 'Completed' || blade.currentEntity.isCancelled;
                //var orderLineItemsBlade = {
                //    id: 'customerOrderItems',
                //    currentEntity: blade.currentEntity,
                //    recalculateFn: blade.recalculate,
                //    controller: 'virtoCommerce.orderModule.customerOrderItemsController',
                //    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-items.tpl.html'
                //};
                //Display order items disabled by default
                // bladeNavigationService.showBlade(orderLineItemsBlade, blade);
            };

            blade.customInitialize();
        }]);
