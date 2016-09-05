angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.customerOrderListController', ['$scope', '$localStorage', 'virtoCommerce.orderModule.order_res_customerOrders', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', 'platformWebApp.authService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'dateFilter', 'virtoCommerce.orderModule.knownOperations',
function ($scope, $localStorage, customerOrders, bladeUtils, dialogService, authService, uiGridConstants, uiGridHelper, dateFilter, knownOperations) {
    var blade = $scope.blade;
    var bladeNavigationService = bladeUtils.bladeNavigationService;
    $scope.uiGridConstants = uiGridConstants;

    blade.refresh = function () {
        blade.isLoading = true;
        var criteria = {
            keyword: filter.keyword,
            sort: uiGridHelper.getSortExpression($scope),
            skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
            take: $scope.pageSettings.itemsPerPageCount
        };
        if (filter.current) {
            angular.extend(criteria, filter.current);
        }

        customerOrders.search(criteria, function (data) {
            blade.isLoading = false;

            $scope.pageSettings.totalItems = data.totalCount;
            $scope.objects = data.customerOrders;
        },
	   function (error) {
	       bladeNavigationService.setError('Error ' + error.status, blade);
	   });
    };

    $scope.selectNode = function (node) {
        $scope.selectedNodeId = node.id;

        var foundTemplate = knownOperations.getOperation(node.operationType);
        if (foundTemplate) {
            var newBlade = angular.copy(foundTemplate.detailBlade);
            newBlade.customerOrder = node;
            bladeNavigationService.showBlade(newBlade, blade);
        }
    };

    $scope.deleteList = function (list) {
        var dialog = {
            id: "confirmDeleteItem",
            title: "orders.dialogs.orders-delete.title",
            message: "orders.dialogs.orders-delete.message",
            callback: function (remove) {
                if (remove) {
                    closeChildrenBlades();

                    var itemIds = _.pluck(list, 'id');
                    customerOrders.remove({ ids: itemIds }, function (data, headers) {
                        blade.refresh();
                    },
                    function (error) {
                        bladeNavigationService.setError('Error ' + error.status, blade);
                    });
                }
            }
        }
        dialogService.showConfirmationDialog(dialog);
    }

    function closeChildrenBlades() {
        angular.forEach(blade.childrenBlades.slice(), function (child) {
            bladeNavigationService.closeBlade(child);
        });
    }

    blade.headIcon = 'fa-file-text';

    blade.toolbarCommands = [
            {
                name: "platform.commands.refresh", icon: 'fa fa-refresh',
                executeMethod: blade.refresh,
                canExecuteMethod: function () {
                    return true;
                }
            },
            {
                name: "platform.commands.delete", icon: 'fa fa-trash-o',
                executeMethod: function () {
                    $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                },
                permission: 'order:delete'
            }
    ];

    // simple and advanced filtering
    var filter = $scope.filter = {};
    var filter = blade.filter = $scope.filter = {};
    $scope.$localStorage = $localStorage;
    if (!$localStorage.orderSearchFilters) {
        $localStorage.orderSearchFilters = [{ name: 'orders.blades.customerOrder-list.labels.new-filter' }]
    }
    if ($localStorage.orderSearchFilterId) {
        filter.current = _.findWhere($localStorage.orderSearchFilters, { id: $localStorage.orderSearchFilterId });
    }

    filter.change = function () {
        $localStorage.orderSearchFilterId = filter.current ? filter.current.id : null;
        if (filter.current && !filter.current.id) {
            filter.current = null;
            showFilterDetailBlade({ isNew: true });
        } else {
            bladeNavigationService.closeBlade({ id: 'filterDetail' });
            filter.criteriaChanged();
        }
    };

    filter.edit = function () {
        if (filter.current) {
            showFilterDetailBlade({ data: filter.current });
        }
    };

    function showFilterDetailBlade(bladeData) {
        var newBlade = {
            id: 'filterDetail',
            controller: 'virtoCommerce.orderModule.filterDetailController',
            template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/filter-detail.tpl.html',
        };
        angular.extend(newBlade, bladeData);
        bladeNavigationService.showBlade(newBlade, blade);
    }

    filter.criteriaChanged = function () {
        if ($scope.pageSettings.currentPage > 1) {
            $scope.pageSettings.currentPage = 1;
        } else {
            blade.refresh();
        }
    };

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        var createdDateColumn = _.findWhere(gridOptions.columnDefs, { name: 'createdDate' });
        if (createdDateColumn) { // custom tooltip
            createdDateColumn.cellTooltip = function (row, col) { return dateFilter(row.entity.createdDate, 'medium'); }
        }
        uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
            uiGridHelper.bindRefreshOnSortChanged($scope);
        });

        bladeUtils.initializePagination($scope);
    };


    // actions on load
    //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
    //blade.refresh();
}]);