angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderDetailController', ['$scope', '$window', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.customerModule.members', 'virtoCommerce.customerModule.memberTypesResolverService', 'platformWebApp.authService',
        function ($scope, $window, bladeNavigationService, dialogService, members, memberTypesResolverService, authService) {
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
                if (blade.customerOrder.customerId) {
                    members.getByUserId({ userId: blade.customerOrder.customerId }, function (member) {
                        if (member && member.id) {
                            showCustomerDetailBlade(member);
                        }
                    });
                }
            };

            blade.openOrganizationDetails = function () {
                if (blade.customerOrder.organizationId) {
                    members.get({ id: blade.customerOrder.organizationId }, function (member) {
                            if (member && member.id) {
                                showCustomerDetailBlade(member);
                            }
                        });
                }
            };

            blade.fetchEmployees = function (criteria) {
                criteria.memberType = 'Employee';
                criteria.deepSearch = true;
                criteria.sort = 'name';

                return members.search(criteria);
            };

            $scope.$on("blade.currentEntity.documentLoaded", function () {
                blade.customInitialize();
            }, true);

            blade.customInitialize = function () {
                if (!blade.currentEntity) {
                    return;
                }
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
