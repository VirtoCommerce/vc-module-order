angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.customerOrderDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.orderModule.order_res_stores', 'platformWebApp.settings', 'virtoCommerce.customerModule.members', 'virtoCommerce.customerModule.memberTypesResolverService',
    function ($scope, bladeNavigationService, dialogService, order_res_stores, settings, members, memberTypesResolverService) {
        var blade = $scope.blade;

        angular.extend(blade, {
            title: 'orders.blades.customerOrder-detail.title',
            titleValues: { customer: blade.customerOrder.customerName },
            subtitle: 'orders.blades.customerOrder-detail.subtitle'
        });

        blade.stores = order_res_stores.query();
        blade.statuses = settings.getValues({ id: 'Order.Status' });
        blade.openStatusSettingManagement = function () {
            var newBlade = new DictionarySettingDetailBlade('Order.Status');
            newBlade.parentRefresh = function (data) { blade.statuses = data; };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        blade.openCustomerDetails = function () {
            var customerMemberType = 'Contact';
            var memberTypeDefinition = _.findWhere(memberTypesResolverService.objects, { memberType: customerMemberType });
            if (memberTypeDefinition) {
                var newBlade = {
                    id: "listMemberDetail",
                    currentEntity: {
                        id: blade.customerOrder.customerId, memberType: customerMemberType
                    },
                    title: blade.customerOrder.customerName,
                    memberTypeDefinition: memberTypeDefinition,
                    controller: memberTypeDefinition.controller,
                    template: memberTypeDefinition.template
                };
                bladeNavigationService.showBlade(newBlade, blade);
            } else {
                dialogService.showNotificationDialog({
                    id: "error",
                    title: "customer.dialogs.unknown-member-type.title",
                    message: "customer.dialogs.unknown-member-type.message",
                    messageValues: { memberType: customerMemberType },
                });
            }
        };

        // load employees
        members.search(
           {
               memberType: 'Employee',
               //memberId: parent org. ID,
               sort: 'fullName:asc',
               take: 1000
           },
           function (data) {
               blade.employees = data.members;
           });

        blade.resetEmployeeName = function (newVal) {
            blade.currentEntity.employeeName = newVal ? newVal.fullName : undefined;
        };
    }]);