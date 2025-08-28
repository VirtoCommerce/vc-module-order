angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderListWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.order_res_customerOrders',
            function ($scope, bladeNavigationService, customerOrders) {
                var blade = $scope.widget.blade;

                var searchCriteria = {};
                if (blade.currentEntity) {
                    if (blade.currentEntity.memberType === "Organization") {
                        searchCriteria.organizationId = blade.currentEntityId;
                    }
                    else {
                        var account = _.first(blade.currentEntity.securityAccounts)
                        if (account) {
                            searchCriteria.customerId = account.id;
                        }
                    }
                }

                function refresh() {
                    $scope.ordersCount = '...';

                    if (!searchCriteria.organizationId && !searchCriteria.customerId) {
                        return;
                    }

                    var countSearchCriteria = {
                        responseGroup: "Default",
                        take: 0
                    };

                    angular.extend(countSearchCriteria, searchCriteria);

                    customerOrders.search(countSearchCriteria, function (data) {
                        $scope.ordersCount = data.totalCount;
                    });
                }

                $scope.openBlade = function () {
                    if (!searchCriteria.organizationId && !searchCriteria.customerId) {
                        return;
                    }

                    var newBlade = {
                        id: 'orders',
                        navigationGroup: 'member_orders',
                        title: 'orders.blades.customerOrder-list.title',
                        searchCriteria: searchCriteria,
                        controller: 'virtoCommerce.orderModule.customerOrderListController',
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-list.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                refresh()
            }]);
