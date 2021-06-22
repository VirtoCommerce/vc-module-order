//Call this to register our module to main application
var moduleName = "virtoCommerce.ordersModule2";

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(
        ['virtoCommerce.orderModule.knownOperations', '$http', '$compile', 'platformWebApp.permissionScopeResolver', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'platformWebApp.ui-grid.extension', '$translate',
            function (knownOperations, $http, $compile, scopeResolver, settings, bladeNavigationService, gridOptionExtension, $translate) {
                var foundTemplate = knownOperations.getOperation('CustomerOrder');
                if (foundTemplate) {
                    foundTemplate.detailBlade.metaFields.push(
                        {
                            name: 'newField',
                            title: "New field",
                            valueType: "ShortText"
                        });

                    foundTemplate.detailBlade.knownChildrenOperations.push('Invoice');
                }

                var invoiceOperation = {
                    type: 'Invoice',
                    description: 'Sample Invoice document',
                    treeTemplateUrl: 'invoiceOperation.tpl.html',
                    detailBlade: {
                        template: 'Modules/$(virtoCommerce.orders2)/Scripts/blades/invoice-detail.tpl.html',
                        metaFields: [
                            {
                                name: 'number',
                                isRequired: true,
                                title: "Invoice number",
                                valueType: "ShortText"
                            },
                            {
                                name: 'createdDate',
                                isReadOnly: true,
                                title: "created",
                                valueType: "DateTime"
                            },
                            {
                                name: 'customerId',
                                title: "Customer",
                                templateUrl: 'customerSelector.html'
                            }
                        ]
                    }
                };
                knownOperations.registerOperation(invoiceOperation);

                $http.get('Modules/$(virtoCommerce.orders2)/Scripts/tree-template.html').then(function (response) {
                    // compile the response, which will put stuff into the cache
                    $compile(response.data);
                });

                //Register permission scopes templates used for scope bounded definition in role management ui
                var orderStatusesScope = {
                    type: 'OrderSelectedStatusScope',
                    title: 'Only for orders in selected statuses',
                    selectFn: function (blade, callback) {
                        var newBlade = {
                            id: 'store-pick',
                            title: this.title,
                            subtitle: 'Select statuses',
                            currentEntity: this,
                            onChangesConfirmedFn: callback,
                            dataPromise: settings.getValues({ id: 'Order.Status' }).$promise.then(arr => _.map(arr, function (value) { return { id: value, name: value } })),
                            controller: 'platformWebApp.security.scopeValuePickFromSimpleListController',
                            template: '$(Platform)/Scripts/app/security/blades/common/scope-value-pick-from-simple-list.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    }
                };
                scopeResolver.register(orderStatusesScope);

                gridOptionExtension.registerExtension("customerOrder-list-grid", function (gridOptions) {
                    var customColumnDefs = [
                        { name: 'newField', displayName: 'orders.blades.customerOrder-list.labels.newField', width: '***' }
                    ];

                    gridOptions.columnDefs = _.union(gridOptions.columnDefs, customColumnDefs);
                });
            }]);
