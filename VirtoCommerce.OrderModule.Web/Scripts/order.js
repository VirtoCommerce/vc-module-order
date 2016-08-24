//Call this to register our module to main application
var moduleName = "virtoCommerce.orderModule";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, ['virtoCommerce.catalogModule', 'virtoCommerce.pricingModule'])
.config(
  ['$stateProvider', function ($stateProvider) {
      $stateProvider
          .state('workspace.orderModule', {
              url: '/orders',
              templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
              controller: [
                  '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                      var blade = {
                          id: 'orders',
                          title: 'orders.blades.customerOrder-list.title',
                          //subtitle: 'Manage Orders',
                          controller: 'virtoCommerce.orderModule.customerOrderListController',
                          template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-list.tpl.html',
                          isClosingDisabled: true
                      };
                      bladeNavigationService.showBlade(blade);
                      //Need for isolate and prevent conflict module css to another modules 
                      //it value included in bladeContainer as ng-class='moduleName'
                      $scope.moduleName = "vc-order";
                  }
              ]
          });
  }]
)
.run(
  ['$rootScope', '$http', '$compile', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', 'platformWebApp.bladeNavigationService', '$state', '$localStorage', 'virtoCommerce.orderModule.order_res_customerOrders', 'platformWebApp.permissionScopeResolver', 'virtoCommerce.storeModule.stores', 'virtoCommerce.customerModule.members',
	function ($rootScope, $http, $compile, mainMenuService, widgetService, bladeNavigationService, $state, $localStorage, customerOrders, scopeResolver, stores, members) {
	    //Register module in main menu
	    var menuItem = {
	        path: 'browse/orders',
	        icon: 'fa fa-file-text',
	        title: 'orders.main-menu-title',
	        priority: 90,
	        action: function () { $state.go('workspace.orderModule'); },
	        permission: 'order:access'
	    };
	    mainMenuService.addMenuItem(menuItem);

	    // register CustomerOrder, PaymentIn and Shipment types as known operations
	    OrderModule_knownOperations.push({
	        type: 'CustomerOrder',
	        treeTemplateUrl: 'orderOperationDefault.tpl.html',
	        getDetailBlade: function (operation, blade) {
	            if (blade.id === 'orderDetail') {
	                bladeNavigationService.closeChildrenBlades(blade);
	            } else {
	                return new OrderModule_orderDetailBlade(operation);
	            }
	        }
	    });

	    var operation = {
	        type: 'PaymentIn',
	        // treeTemplateUrl: 'orderOperationDefault.tpl.html',
	        getDetailBlade: function (operation, blade) { return new OrderModule_paymentDetailBlade(operation, blade); },
	        newInstanceMetadata: {
	            name: 'orders.blades.newOperation-wizard.menu.payment-operation.title',
	            descr: 'orders.blades.newOperation-wizard.menu.payment-operation.description',
	            action: function (blade) {
	                customerOrders.getNewPayment({ id: blade.customerOrder.id }, function (result) {
	                    bladeNavigationService.closeBlade(blade);

	                    result.paymentMethod = undefined;
	                    blade.customerOrder.inPayments.push(result);
	                    blade.customerOrder.childrenOperations.push(result);

	                    var newBlade = operation.getDetailBlade(result, blade);
	                    newBlade.isNew = true;
	                    bladeNavigationService.showBlade(newBlade, blade.parentBlade);
	                },
                    function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
	            }
	        }
	    }
	    OrderModule_knownOperations.push(operation);

	    operation = {
	        type: 'Shipment',
	        getDetailBlade: function (operation, blade) { return new OrderModule_shipmentDetailBlade(operation, blade); },
	        newInstanceMetadata: {
	            name: 'orders.blades.newOperation-wizard.menu.shipment-operation.title',
	            descr: 'orders.blades.newOperation-wizard.menu.shipment-operation.description',
	            action: function (blade) {
	                customerOrders.getNewShipment({ id: blade.customerOrder.id }, function (result) {
	                    bladeNavigationService.closeBlade(blade);

	                    result.shippingMethod = undefined;
	                    blade.customerOrder.shipments.push(result);
	                    blade.customerOrder.childrenOperations.push(result);

	                    var newBlade = operation.getDetailBlade(result, blade);
	                    newBlade.isNew = true;
	                    bladeNavigationService.showBlade(newBlade, blade.parentBlade);
	                },
                    function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
	            }
	        }
	    };
	    OrderModule_knownOperations.push(operation);

	    //Register widgets
	    var operationItemsWidget = {
	        controller: 'virtoCommerce.orderModule.customerOrderItemsWidgetController',
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-items-widget.tpl.html'
	    };
	    widgetService.registerWidget(operationItemsWidget, 'customerOrderDetailWidgets');

	    var shipmentItemsWidget = {
	        controller: 'virtoCommerce.orderModule.shipmentItemsWidgetController',
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/shipment-items-widget.tpl.html'
	    };
	    widgetService.registerWidget(shipmentItemsWidget, 'shipmentDetailWidgets');


	    var customerOrderAddressWidget = {
	        controller: 'virtoCommerce.orderModule.customerOrderAddressWidgetController',
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-address-widget.tpl.html'
	    };
	    widgetService.registerWidget(customerOrderAddressWidget, 'customerOrderDetailWidgets');

	    var customerOrderTotalsWidget = {
	        controller: 'virtoCommerce.orderModule.customerOrderTotalsWidgetController',
	        size: [2, 2],
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-totals-widget.tpl.html'
	    };
	    widgetService.registerWidget(customerOrderTotalsWidget, 'customerOrderDetailWidgets');


	    var operationCommentWidget = {
	        controller: 'virtoCommerce.orderModule.operationCommentWidgetController',
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/operation-comment-widget.tpl.html'
	    };
	    widgetService.registerWidget(operationCommentWidget, 'customerOrderDetailWidgets');

	    var shipmentAddressWidget = {
	        controller: 'virtoCommerce.orderModule.shipmentAddressWidgetController',
	        size: [2, 1],
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/shipment-address-widget.tpl.html'
	    };
	    widgetService.registerWidget(shipmentAddressWidget, 'shipmentDetailWidgets');


	    var shipmentTotalWidget = {
	        controller: 'virtoCommerce.orderModule.shipmentTotalsWidgetController',
	        size: [2, 1],
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/shipment-totals-widget.tpl.html'
	    };
	    widgetService.registerWidget(shipmentTotalWidget, 'shipmentDetailWidgets');

	    var dynamicPropertyWidget = {
	        controller: 'platformWebApp.dynamicPropertyWidgetController',
	        template: '$(Platform)/Scripts/app/dynamicProperties/widgets/dynamicPropertyWidget.tpl.html'
	    };
	    widgetService.registerWidget(dynamicPropertyWidget, 'shipmentDetailWidgets');
	    widgetService.registerWidget(dynamicPropertyWidget, 'customerOrderDetailWidgets');
	    widgetService.registerWidget(dynamicPropertyWidget, 'paymentDetailWidgets');


	    var operationsTreeWidget = {
	        controller: 'virtoCommerce.orderModule.operationTreeWidgetController',
	        size: [4, 3],
	        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/operation-tree-widget.tpl.html'
	    };
	    widgetService.registerWidget(operationsTreeWidget, 'customerOrderDetailWidgets');

	    // register dashboard widgets
	    var statisticsController = 'virtoCommerce.orderModule.dashboard.statisticsWidgetController';
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [2, 1],
	        template: 'order-statistics-revenue.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [2, 1],
	        template: 'order-statistics-customersCount.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [2, 1],
	        template: 'order-statistics-revenuePerCustomer.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [2, 1],
	        template: 'order-statistics-orderValue.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [2, 1],
	        template: 'order-statistics-itemsPurchased.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [2, 1],
	        template: 'order-statistics-lineitemsPerOrder.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [3, 2],
	        template: 'order-statistics-revenueByQuarter.html'
	    }, 'mainDashboard');
	    widgetService.registerWidget({
	        controller: statisticsController,
	        size: [3, 2],
	        template: 'order-statistics-orderValueByQuarter.html'
	    }, 'mainDashboard');

	    $http.get('Modules/$(VirtoCommerce.Orders)/Scripts/widgets/dashboard/statistics-templates.html').then(function (response) {
	        // compile the response, which will put stuff into the cache
	        $compile(response.data);
	    });


	    //Register permission scopes templates used for scope bounded definition in role management ui
	    var orderStoreScope = {
	        type: 'OrderStoreScope',
	        title: 'Only for orders in selected stores',
	        selectFn: function (blade, callback) {
	            var newBlade = {
	                id: 'store-pick',
	                title: this.title,
	                subtitle: 'Select stores',
	                currentEntity: this,
	                onChangesConfirmedFn: callback,
	                dataPromise: stores.query().$promise,
	                controller: 'platformWebApp.security.scopeValuePickFromSimpleListController',
	                template: '$(Platform)/Scripts/app/security/blades/common/scope-value-pick-from-simple-list.tpl.html'
	            };
	            bladeNavigationService.showBlade(newBlade, blade);
	        }
	    };
	    scopeResolver.register(orderStoreScope);
	    var responsibleOrderScope = {
	        type: 'OrderResponsibleScope',
	        title: 'Only for order responsible',
	    };
	    scopeResolver.register(responsibleOrderScope);

	    $rootScope.$on('loginStatusChanged', function (event, authContext) {
	        if (authContext.isAuthenticated) {
	            var now = new Date();
	            var startDate = new Date();
	            startDate.setFullYear(now.getFullYear() - 1);

	            customerOrders.getDashboardStatistics({ start: startDate, end: now }, function (data) {
	                // prepare statistics
	                var statisticsToChartRows = function (statsList, allCurrencies) {
	                    var groupedQuarters = _.groupBy(statsList, function (stats) { return stats.year + ' Q' + stats.quarter; });
	                    return _.map(groupedQuarters, function (stats, key) {
	                        var values = [{ v: key }];
	                        _.each(allCurrencies, function (x) {
	                            var stat = _.findWhere(stats, { currency: x });
	                            values.push({ v: stat ? stat.amount : 0 });
	                        });
	                        return {
	                            c: values
	                        };
	                    });
	                }

	                var allCurrencies = _.unique(_.pluck(data.avgOrderValuePeriodDetails, 'currency').sort());

	                var cols = [{ id: "quarter", label: "Quarter", type: "string" }];
	                _.each(allCurrencies, function (x) {
	                    cols.push({ id: "revenue" + x, label: x, type: "number" });
	                });

	                data.chartRevenueByQuarter = {
	                    "type": "LineChart",
	                    "data": {
	                        cols: cols,
	                        rows: statisticsToChartRows(data.revenuePeriodDetails, allCurrencies)
	                    },
	                    "options": {
	                        "title": "Revenue by quarter",
	                        "legend": { position: 'top' },
	                        "vAxis": {
	                            // "title": "Sales unit",
	                            gridlines: { count: 8 }
	                        },
	                        "hAxis": {
	                            // "title": "Date"
	                            slantedText: true,
	                            slantedTextAngle: 20
	                        }
	                    },
	                    "formatters": {}
	                };

	                cols = [{ id: "quarter", label: "Quarter", type: "string" }];
	                _.each(allCurrencies, function (x) {
	                    cols.push({ id: "avg-orderValue" + x, label: x, type: "number" });
	                });

	                data.chartOrderValueByQuarter = {
	                    "type": "ColumnChart",
	                    "data": {
	                        cols: cols,
	                        rows: statisticsToChartRows(data.avgOrderValuePeriodDetails, allCurrencies)
	                    },
	                    "options": {
	                        "title": "Average Order value by quarter",
	                        "legend": { position: 'top' },
	                        "vAxis": {
	                            gridlines: { count: 8 }
	                        },
	                        "hAxis": {
	                            slantedText: true,
	                            slantedTextAngle: 20
	                        }
	                    },
	                    "formatters": {}
	                };

	                $localStorage.ordersDashboardStatistics = data;
	            },
                function (error) {
                    console.log(error);
                });
	        }
	    });
	}]);

// define OrderModule known Operations to be accessible platform-wide:
var OrderModule_knownOperations = [];

// define base blade information for operations
function OrderModule_operationDetailBlade() {
    this.id = 'operationDetail';
    this.controller = 'virtoCommerce.orderModule.operationDetailController';
}

// define ORDER detail blade
function OrderModule_orderDetailBlade(node) {
    this.id = 'orderDetail';
    this.customerOrder = node;
    this.title = 'orders.blades.customerOrder-detail.title';
    this.titleValues = { customer: node.customerName };
    this.subtitle = 'orders.blades.customerOrder-detail.subtitle';
    this.template = 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-detail.tpl.html';
    this.availableChildrenTypes = ['Shipment', 'PaymentIn'];
}

// inherit form base operation
OrderModule_orderDetailBlade.prototype = new OrderModule_operationDetailBlade();

// register order metaFields
OrderModule_orderDetailBlade.prototype.metaFields = [
    {
        name: 'isApproved',
        title: "orders.blades.customerOrder-detail.labels.approved",
        valueType: "Boolean",
        isVisibleFn: function (blade) { return !blade.isNew; }
        //initializeFn: function (blade) { this.values = [{ value: blade.currentEntity.isApproved }]; },
        // dataSaveFn: function (blade) { return blade.currentEntity.isApproved = this.values[0].value; }
    },
    {
        name: 'employeeId',
        title: "orders.blades.customerOrder-detail.labels.employee",
        templateUrl: 'employeeSelector.html'
    },
    {
        name: 'number',
        isRequired: true,
        title: "orders.blades.customerOrder-detail.labels.order-number",
        valueType: "ShortText",
        isVisibleFn: function (blade) { return !blade.isNew; }
    },
    {
        name: 'createdDate',
        isReadonly: true,
        title: "orders.blades.customerOrder-detail.labels.from",
        valueType: "DateTime"
    },
    {
        name: 'status',
        templateUrl: 'statusSelector.html'
    },
    {
        name: 'customerName',
        title: "orders.blades.customerOrder-detail.labels.customer",
        templateUrl: 'customerSelector.html'
    },
   {
       name: 'storeId',
       title: "orders.blades.customerOrder-detail.labels.store",
       templateUrl: 'storeSelector.html'
   }
];

// PAYMENT detail blade
function OrderModule_paymentDetailBlade(operation, blade) {
    this.currentEntity = operation;
    this.customerOrder = blade.customerOrder;
    this.stores = blade.stores;
    this.title = 'orders.blades.payment-detail.title';
    this.titleValues = { number: operation.number };
    this.subtitle = 'orders.blades.payment-detail.subtitle';
    this.template = 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/payment-detail.tpl.html';
}

// inherit form base operation
OrderModule_paymentDetailBlade.prototype = new OrderModule_operationDetailBlade();

OrderModule_paymentDetailBlade.prototype.metaFields = [
    {
        name: 'number',
        isRequired: true,
        title: "orders.blades.payment-detail.labels.payment-number",
        valueType: "ShortText"
    },
    {
        name: 'createdDate',
        isReadonly: true,
        title: "orders.blades.payment-detail.labels.from",
        valueType: "DateTime"
    }
];

// SHIPMENT detail blade
function OrderModule_shipmentDetailBlade(operation, blade) {
    this.currentEntity = operation;
    this.customerOrder = blade.customerOrder;
    this.stores = blade.stores;
    this.title = 'orders.blades.shipment-detail.title';
    this.titleValues = { number: operation.number };
    this.subtitle = 'orders.blades.shipment-detail.subtitle';
    this.template = 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/shipment-detail.tpl.html';
}

// inherit form base operation
OrderModule_shipmentDetailBlade.prototype = new OrderModule_operationDetailBlade();
