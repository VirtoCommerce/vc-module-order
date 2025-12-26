//Call this to register our module to main application
var moduleName = "virtoCommerce.orderModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [
    'virtoCommerce.pricingModule',
    'virtoCommerce.customerModule',
    'virtoCommerce.storeModule',
    'virtoCommerce.inventoryModule'])
    .config(
        ['$stateProvider', function ($stateProvider) {
            $stateProvider
                .state('workspace.orderModule', {
                    url: '/orders',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        '$scope', '$location', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.knownOperations', function ($scope, $location, bladeNavigationService, knownOperations) {
                            var orderId = $location.search().orderId;

                            if (orderId) {
                                var foundTemplate = knownOperations.getOperation('CustomerOrder');
                                if (foundTemplate) {
                                    var newBlade = angular.copy(foundTemplate.detailBlade);
                                    newBlade.id = 'orders';
                                    newBlade.customerOrder = { id: orderId, customerName: 'Customer' };
                                    newBlade.isClosingDisabled = true;
                                    bladeNavigationService.showBlade(newBlade);
                                }
                            }
                            else {
                                var blade = {
                                    id: 'orders',
                                    title: 'orders.blades.customerOrder-list.title',
                                    //subtitle: 'Manage Orders',
                                    controller: 'virtoCommerce.orderModule.customerOrderListController',
                                    isExpandable: true,
                                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-list.tpl.html',
                                    isClosingDisabled: true
                                };
                                bladeNavigationService.showBlade(blade);
                            }
                            //Need for isolate and prevent conflict module css to another modules
                            //it value included in bladeContainer as ng-class='moduleName'
                            $scope.moduleName = "vc-order";
                        }
                    ]
                });
        }]
    )
    // define known Operations to be accessible platform-wide
    .factory('virtoCommerce.orderModule.knownOperations', function () {
        var map = {};

        function registerOperation(op) {
            var copy = angular.copy(op);
            copy.detailBlade = angular.extend({
                id: 'operationDetail',
                knownChildrenOperations: [],
                metaFields: [],
                isLocked: false,
                controller: 'virtoCommerce.orderModule.operationDetailController'
            }, copy.detailBlade);

            map[op.type] = copy;
        }

        function getOperation(type) {
            return map[type];
        }

        function isLocked(type, entity) {
            var operation = getOperation(type);
            if (typeof operation.isLocked === "function" && entity) {
                return operation.isLocked(entity);
            }
            return false;
        }

        return {
            registerOperation: registerOperation,
            getOperation: getOperation,
            isLocked: isLocked
        };
    })
    .run(
        [
            '$rootScope',
            '$http',
            '$compile',
            'platformWebApp.mainMenuService',
            'platformWebApp.widgetService',
            'platformWebApp.bladeNavigationService',
            '$state',
            '$localStorage',
            'virtoCommerce.orderModule.order_res_customerOrders',
            'platformWebApp.permissionScopeResolver',
            'virtoCommerce.storeModule.stores',
            'virtoCommerce.orderModule.knownOperations',
            'platformWebApp.authService',
            'platformWebApp.metaFormsService',
            function ( // nosonar
                $rootScope,
                $http,
                $compile,
                mainMenuService,
                widgetService,
                bladeNavigationService,
                $state,
                $localStorage,
                customerOrders,
                scopeResolver,
                stores,
                knownOperations,
                authService,
                metaFormsService) {
                //Register module in main menu
                var menuItem = {
                    path: 'browse/orders',
                    icon: 'fa fa-file-text',
                    title: 'orders.main-menu-title',
                    priority: 90,
                    action: function () { $state.go('workspace.orderModule', {}, { reload: true }); },
                    permission: 'order:access'
                };
                mainMenuService.addMenuItem(menuItem);

                // register CustomerOrder, PaymentIn and Shipment types as known operations
                knownOperations.registerOperation({
                    type: 'CustomerOrder',
                    treeTemplateUrl: 'orderOperationDefault.tpl.html',
                    isLocked: function (entity) {
                        return entity.status === 'Completed' ||
                            entity.cancelledState === 'Completed' ||
                            entity.isCancelled;
                    },
                    detailBlade: {
                        id: 'orderDetail',
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-detail.tpl.html',
                        knownChildrenOperations: ['Shipment', 'PaymentIn'],
                        metaFields: [
                            {
                                name: 'number',
                                isRequired: true,
                                title: "orders.blades.customerOrder-detail.labels.order-number",
                                valueType: "ShortText"
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
                                name: 'organizationName',
                                title: "orders.blades.customerOrder-detail.labels.organization",
                                templateUrl: 'organizationSelector.html'
                            },
                            {
                                name: 'storeId',
                                title: "orders.blades.customerOrder-detail.labels.store",
                                templateUrl: 'storeSelector.html'
                            },
                            {
                                name: 'createdDate',
                                isReadOnly: true,
                                title: "orders.blades.customerOrder-detail.labels.from",
                                valueType: "DateTime"
                            },
                            {
                                name: 'employeeId',
                                title: "orders.blades.customerOrder-detail.labels.employee",
                                templateUrl: 'order-employeeSelector.html'
                            },
                            {
                                name: 'discountAmount',
                                title: "orders.blades.customerOrder-items.labels.discount",
                                templateUrl: 'discountAmount.html'
                            },
                            {
                                name: 'isApproved',
                                title: "orders.blades.customerOrder-detail.labels.approved",
                                valueType: "Boolean",
                                isVisibleFn: function (blade) {
                                    return !blade.isNew;
                                }
                            }
                        ]
                    }
                });

                var paymentInOperation = {
                    type: 'PaymentIn',
                    description: 'orders.blades.newOperation-wizard.menu.payment-operation.description',
                    isLocked: function (entity) {
                        return entity.status === 'Paid'
                            || entity.cancelledState === 'Requested'
                            || entity.cancelledState === 'Completed'
                            || entity.isCancelled;
                    },
                    detailBlade: {
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/payment-detail.tpl.html',
                        metaFields: [
                            {
                                name: 'number',
                                isRequired: true,
                                title: "orders.blades.payment-detail.labels.payment-number",
                                valueType: "ShortText"
                            },
                            {
                                name: 'createdDate',
                                isReadOnly: true,
                                title: "orders.blades.payment-detail.labels.from",
                                valueType: "DateTime"
                            },
                            {
                                name: 'price',
                                title: "orders.blades.payment-detail.labels.price",
                                templateUrl: 'price.html'
                            },
                            {
                                name: 'priceWithTax',
                                title: "orders.blades.payment-detail.labels.price-with-tax",
                                templateUrl: 'priceWithTax.html'
                            }
                        ]
                    }
                };
                knownOperations.registerOperation(paymentInOperation);

                var refundOperation = {
                    type: 'Refund',
                    description: 'orders.blades.newOperation-wizard.menu.refund-operation',
                    detailBlade: {
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/refund-detail.html',
                        metaFields: [
                            {
                                name: 'number',
                                isRequired: true,
                                title: "orders.blades.payment-detail.labels.payment-number",
                                valueType: "ShortText"
                            },
                            {
                                name: 'createdDate',
                                isReadOnly: true,
                                title: "orders.blades.payment-detail.labels.from",
                                valueType: "DateTime"
                            },
                            {
                                name: 'transactionId',
                                isReadOnly: true,
                                title: "orders.blades.refund-detail.labels.transactionId",
                                valueType: "ShortText"
                            },
                            {
                                name: 'outerId',
                                isReadOnly: true,
                                title: "orders.blades.refund-detail.labels.outerId",
                                valueType: "ShortText"
                            }
                        ]
                    }
                };
                knownOperations.registerOperation(refundOperation);

                var captureOperation = {
                    type: 'Capture',
                    description: 'orders.blades.newOperation-wizard.menu.capture-operation',
                    detailBlade: {
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/capture-detail.html',
                        metaFields: [
                            {
                                name: 'number',
                                isRequired: true,
                                title: "orders.blades.payment-detail.labels.payment-number",
                                valueType: "ShortText"
                            },
                            {
                                name: 'createdDate',
                                isReadOnly: true,
                                title: "orders.blades.payment-detail.labels.from",
                                valueType: "DateTime"
                            },
                            {
                                name: 'transactionId',
                                isReadOnly: true,
                                title: "orders.blades.capture-detail.labels.transactionId",
                                valueType: "ShortText"
                            },
                            {
                                name: 'closeTransaction',
                                isReadOnly: true,
                                title: 'orders.blades.capture-details.labels.closeTransaction',
                                valueType: "ShortText"
                            },
                            {
                                name: 'outerId',
                                isReadOnly: true,
                                title: "orders.blades.capture-detail.labels.outerId",
                                valueType: "ShortText"
                            }
                        ]
                    }
                };
                knownOperations.registerOperation(captureOperation);

                var shipmentOperation = {
                    type: 'Shipment',
                    description: 'orders.blades.newOperation-wizard.menu.shipment-operation.description',
                    treeTemplateUrl: 'shipmentOperationDefault.tpl.html',
                    isLocked: function (entity) {
                        return entity.status === 'Send'
                            || entity.cancelledState === 'Completed'
                            || entity.cancelledState === 'Requested'
                            || entity.isCancelled;
                    },
                    detailBlade: {
                        template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/shipment-detail.tpl.html',
                        metaFields: [
                            {
                                name: 'number',
                                isRequired: true,
                                title: "orders.blades.shipment-detail.labels.shipment-number",
                                valueType: "ShortText"
                            },
                            {
                                name: 'createdDate',
                                isReadOnly: true,
                                title: "orders.blades.shipment-detail.labels.from",
                                valueType: "DateTime"
                            },
                            {
                                name: 'status',
                                templateUrl: 'statusSelector.html'
                            },
                            {
                                name: 'employeeId',
                                title: "orders.blades.shipment-detail.labels.employee",
                                templateUrl: 'shipment-employeeSelector.html'
                            },
                            {
                                name: 'price',
                                title: "orders.blades.shipment-detail.labels.price",
                                templateUrl: 'price.html'
                            },
                            {
                                name: 'priceWithTax',
                                title: "orders.blades.shipment-detail.labels.price-with-tax",
                                templateUrl: 'priceWithTax.html'
                            },
                            {
                                name: 'trackingNumber',
                                title: 'orders.blades.shipment-detail.labels.tracking-number',
                                valueType: 'ShortText'
                            },
                            {
                                name: 'deliveryDate',
                                title: 'orders.blades.shipment-detail.labels.delivery-date',
                                valueType: 'DateTime'
                            },
                            {
                                name: 'trackingUrl',
                                title: 'orders.blades.shipment-detail.labels.tracking-url',
                                valueType: 'LongText'
                            },
                            {
                                name: 'vendorId',
                                templateUrl: "vendor.html"
                            }
                        ]
                    }
                };
                knownOperations.registerOperation(shipmentOperation);

                //Register widgets
                widgetService.registerWidget({
                    controller: 'virtoCommerce.orderModule.notificationsLogWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/notificationsLogWidget.tpl.html',
                    isVisible: function (blade) { return authService.checkPermission('notifications:access'); }
                }, 'customerOrderDetailWidgets');

                var operationItemsWidget = {
                    controller: 'virtoCommerce.orderModule.customerOrderItemsWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-items-widget.tpl.html'
                };
                widgetService.registerWidget(operationItemsWidget, 'customerOrderDetailWidgets');

                widgetService.registerWidget({
                    controller: 'virtoCommerce.orderModule.customerOrderChangeLogWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-change-log-widget.tpl.html'
                }, 'customerOrderDetailWidgets');

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
                function checkPermissionToReadPrices() {
                    return authService.checkPermission('order:read_prices');
                }

                function checkPermissionToViewDashboard() {
                    return authService.checkPermission('order:dashboardstatistics:view') ||
                        $localStorage.ordersDashboardStatistics != null;
                }

                var customerOrderTotalsWidget = {
                    controller: 'virtoCommerce.orderModule.customerOrderTotalsWidgetController',
                    size: [2, 2],
                    isVisible: checkPermissionToReadPrices,
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-totals-widget.tpl.html'
                };
                widgetService.registerWidget(customerOrderTotalsWidget, 'customerOrderDetailWidgets');


                var operationCommentWidget = {
                    controller: 'virtoCommerce.orderModule.operationCommentWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/operation-comment-widget.tpl.html'
                };
                widgetService.registerWidget(operationCommentWidget, 'customerOrderDetailWidgets');
                widgetService.registerWidget(operationCommentWidget, 'shipmentDetailWidgets');
                widgetService.registerWidget(operationCommentWidget, 'paymentDetailWidgets');
                widgetService.registerWidget(operationCommentWidget, 'refundDetailWidgets');
                widgetService.registerWidget(operationCommentWidget, 'captureDetailWidgets');


                var shipmentAddressWidget = {
                    controller: 'virtoCommerce.orderModule.shipmentAddressWidgetController',
                    size: [2, 1],
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/shipment-address-widget.tpl.html'
                };
                widgetService.registerWidget(shipmentAddressWidget, 'shipmentDetailWidgets');


                var shipmentTotalWidget = {
                    controller: 'virtoCommerce.orderModule.shipmentTotalsWidgetController',
                    size: [2, 1],
                    isVisible: checkPermissionToReadPrices,
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/shipment-totals-widget.tpl.html'
                };
                widgetService.registerWidget(shipmentTotalWidget, 'shipmentDetailWidgets');

                widgetService.registerWidget({
                    controller: 'virtoCommerce.orderModule.paymentAddressWidgetController',
                    size: [2, 1],
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/payment-address-widget.tpl.html'
                }, 'paymentDetailWidgets');

                var paymentTotalWidget = {
                    controller: 'virtoCommerce.orderModule.paymentTotalsWidgetController',
                    size: [2, 1],
                    isVisible: checkPermissionToReadPrices,
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/payment-totals-widget.tpl.html'
                };
                widgetService.registerWidget(paymentTotalWidget, 'paymentDetailWidgets');

                var paymentTransactionsWidget = {
                    controller: 'virtoCommerce.orderModule.paymentTransactionsWidgetController',
                    size: [1, 1],
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/payment-transactions-widget.tpl.html'
                };
                widgetService.registerWidget(paymentTransactionsWidget, 'paymentDetailWidgets');

                var operationsWidget = {
                    controller: 'platformWebApp.changeLog.operationsWidgetController',
                    template: '$(Platform)/Scripts/app/changeLog/widgets/operations-widget.tpl.html'
                };
                widgetService.registerWidget(operationsWidget, 'shipmentDetailWidgets');
                widgetService.registerWidget(operationsWidget, 'paymentDetailWidgets');
                widgetService.registerWidget(operationsWidget, 'refundDetailWidgets');
                widgetService.registerWidget(operationsWidget, 'captureDetailWidgets');

                var dynamicPropertyWidget = {
                    controller: 'platformWebApp.dynamicPropertyWidgetController',
                    template: '$(Platform)/Scripts/app/dynamicProperties/widgets/dynamicPropertyWidget.tpl.html',
                    isVisible: function (blade) { return authService.checkPermission('platform:dynamic_properties:read'); }
                };
                widgetService.registerWidget(dynamicPropertyWidget, 'shipmentDetailWidgets');
                widgetService.registerWidget(dynamicPropertyWidget, 'customerOrderDetailWidgets');
                widgetService.registerWidget(dynamicPropertyWidget, 'paymentDetailWidgets');
                widgetService.registerWidget(dynamicPropertyWidget, 'refundDetailWidgets');
                widgetService.registerWidget(dynamicPropertyWidget, 'captureDetailWidgets');
                widgetService.registerWidget(dynamicPropertyWidget, 'customerOrderItemDetailWidgets');

                var operationsTreeWidget = {
                    controller: 'virtoCommerce.orderModule.operationTreeWidgetController',
                    size: [4, 3],
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/operation-tree-widget.tpl.html'
                };
                widgetService.registerWidget(operationsTreeWidget, 'customerOrderDetailWidgets');

                // register members widgets
                var customerOrdersWidget = {
                    controller: 'virtoCommerce.orderModule.customerOrderListWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-list-widget.tpl.html',
                    isVisible: function (blade) { return !blade.isNew; }
                };

                widgetService.registerWidget(customerOrdersWidget, 'customerDetail1');
                widgetService.registerWidget(customerOrdersWidget, 'organizationDetail1');

                // register dashboard widgets
                var statisticsController = 'virtoCommerce.orderModule.dashboard.statisticsWidgetController';

                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [2, 1],
                    template: 'order-statistics-revenue.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [2, 1],
                    template: 'order-statistics-customersCount.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [2, 1],
                    template: 'order-statistics-revenuePerCustomer.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [2, 1],
                    template: 'order-statistics-orderValue.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [2, 1],
                    template: 'order-statistics-itemsPurchased.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [2, 1],
                    template: 'order-statistics-lineItemsPerOrder.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [3, 2],
                    template: 'order-statistics-revenueByQuarter.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [3, 2],
                    template: 'order-statistics-orderValueByQuarter.html',
                    isVisible: checkPermissionToViewDashboard
                }, 'mainDashboard');
                widgetService.registerWidget({
                    controller: statisticsController,
                    size: [6, 1],
                    template: 'order-statistics-not-authorized.html',
                    isVisible: function () { return !checkPermissionToViewDashboard(); }
                }, 'mainDashboard');

                $http.get('Modules/$(VirtoCommerce.Orders)/Scripts/widgets/dashboard/statistics-templates.html').then(function (response) {
                    // compile the response, which will put stuff into the cache
                    $compile(response.data);
                });

                metaFormsService.registerMetaFields('orderFilterDetail', [
                    {
                        name: 'statuses',
                        title: "orders.blades.customerOrder-detail.labels.status",
                        templateUrl: 'statusesSelector.html'
                    },
                    {
                        name: 'storeIds',
                        title: "orders.blades.customerOrder-detail.labels.store",
                        templateUrl: 'storeSelector.html'
                    },
                    {
                        name: 'customerId',
                        title: "orders.blades.customerOrder-detail.labels.customer",
                        templateUrl: 'customerSelector.html'
                    },
                    {
                        name: 'organizationId',
                        title: "orders.blades.customerOrder-detail.labels.organization",
                        templateUrl: 'filter-organizationSelector.html'
                    },
                    {
                        name: 'startDate',
                        title: "orders.blades.filter-detail.labels.from",
                        valueType: "DateTime"
                    },
                    {
                        name: 'endDate',
                        title: "orders.blades.filter-detail.labels.to",
                        valueType: "DateTime"
                    },
                    {
                        name: 'employeeId',
                        title: "orders.blades.shipment-detail.labels.employee",
                        templateUrl: 'filter-employeeSelector.html'
                    },
                    {
                        name: 'withPrototypes',
                        title: "orders.blades.customerOrder-detail.labels.withPrototypes",
                        templateUrl: 'filter-withPrototypes.html'
                    }
                ]);

                //Register permission scopes templates used for scope bounded definition in role management ui
                var orderStoreScope = {
                    type: 'OrderSelectedStoreScope',
                    title: 'Only for orders in selected stores',
                    selectFn: function (blade, callback) {
                        var newBlade = {
                            id: 'store-pick',
                            title: this.title,
                            subtitle: 'Select stores',
                            currentEntity: this,
                            onChangesConfirmedFn: callback,
                            dataService: stores,
                            controller: 'platformWebApp.security.scopeValuePickFromSimpleListController',
                            template: '$(Platform)/Scripts/app/security/blades/common/scope-value-pick-from-simple-list.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    }
                };
                scopeResolver.register(orderStoreScope);
                var responsibleOrderScope = {
                    type: 'OnlyOrderResponsibleScope',
                    title: 'Only for order responsible'
                };
                scopeResolver.register(responsibleOrderScope);

                $rootScope.$on('loginStatusChanged', function (event, authContext) {
                    if (authContext.isAuthenticated) {
                        customerOrders.indexedSearchEnabled(function (data) {
                            if (data.result) {
                                var customerOrderIndexWidget = {
                                    documentType: 'CustomerOrder',
                                    controller: 'virtoCommerce.searchModule.indexWidgetController',
                                    template: 'Modules/$(VirtoCommerce.Search)/Scripts/widgets/index-widget.tpl.html',
                                    isVisible: function (blade) { return !blade.isNew; }
                                };
                                widgetService.registerWidget(customerOrderIndexWidget, 'customerOrderDetailWidgets');
                            }
                        });
                    }

                    $localStorage.ordersDashboardStatistics = null;

                    if (authContext.isAuthenticated &&
                        authService.checkPermission('order:dashboardstatistics:view')) {
                        var now = new Date();
                        var startDate = new Date();
                        startDate.setFullYear(now.getFullYear() - 1);

                        customerOrders.getDashboardStatistics({ start: startDate, end: now }, function (data) {
                            // prepare statistics
                            var statisticsToChartRows = function (statsList, allCurrencies) {
                                var groupedQuarters = _.groupBy(statsList, function (stats) {
                                    return `${stats.year} Q${stats.quarter}`;
                                });
                                return _.map(groupedQuarters, function (stats, key) {
                                    var values = [{
                                        v: key
                                    }];
                                    _.each(allCurrencies, function (x) {
                                        var stat = _.findWhere(stats, {
                                            currency: x
                                        });
                                        values.push({
                                            v: stat ? stat.amount : 0
                                        });
                                    });
                                    return {
                                        c: values
                                    };
                                });
                            };

                            var allCurrencies = _.unique(_.pluck(data.avgOrderValuePeriodDetails, 'currency').sort());

                            var cols = [{
                                id: "quarter", label: "Quarter", type: "string"
                            }];
                            _.each(allCurrencies, function (x) {
                                cols.push({
                                    id: "revenue" + x, label: x, type: "number"
                                });
                            });

                            data.chartRevenueByQuarter = {
                                "type": "LineChart",
                                "data": {
                                    cols: cols,
                                    rows: statisticsToChartRows(data.revenuePeriodDetails, allCurrencies)
                                },
                                "options": {
                                    "title": "Revenue by quarter",
                                    "legend": {
                                        position: 'top'
                                    },
                                    "vAxis": {
                                        // "title": "Sales unit",
                                        gridlines: {
                                            count: 8
                                        }
                                    },
                                    "hAxis": {
                                        // "title": "Date"
                                        slantedText: true,
                                        slantedTextAngle: 20
                                    }
                                },
                                "formatters": {}
                            };

                            cols = [{
                                id: "quarter", label: "Quarter", type: "string"
                            }];
                            _.each(allCurrencies, function (x) {
                                cols.push({
                                    id: "avg-orderValue" + x, label: x, type: "number"
                                });
                            });

                            data.chartOrderValueByQuarter = {
                                "type": "ColumnChart",
                                "data": {
                                    cols: cols,
                                    rows: statisticsToChartRows(data.avgOrderValuePeriodDetails, allCurrencies)
                                },
                                "options": {
                                    "title": "Average Order value by quarter",
                                    "legend": {
                                        position: 'top'
                                    },
                                    "vAxis": {
                                        gridlines: {
                                            count: 8
                                        }
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

                var customerOrderItemDiscountWidget = {
                    controller: 'virtoCommerce.orderModule.customerOrderItemDiscountWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-item-discounts-widget.tpl.html'
                };
                widgetService.registerWidget(customerOrderItemDiscountWidget, 'customerOrderItemDetailWidgets');

                var customerOrderItemConfigurationWidget = {
                    controller: 'virtoCommerce.orderModule.customerOrderItemConfigurationWidgetController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/widgets/customerOrder-item-configuration-widget.tpl.html',
                    isVisible: function (blade) { return blade.currentEntity.configurationItems?.length && authService.checkPermission('catalog:configurations:read'); }
                };
                widgetService.registerWidget(customerOrderItemConfigurationWidget, 'customerOrderItemDetailWidgets');
            }]);
