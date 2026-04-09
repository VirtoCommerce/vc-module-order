angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemDetailController', [
        '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.authService', 'virtoCommerce.orderModule.productBladeResolver',
        function ($scope, bladeNavigationService, authService, productBladeResolver) {
            var blade = $scope.blade;
            blade.updatePermission = 'order:update';
            blade.isVisiblePrices = authService.checkPermission('order:read_prices');
            blade.formScope = null;
            blade.metaFields = [
                {
                    title: "orders.blades.customerOrder-item-detail.labels.product",
                    colSpan: 2,
                    templateUrl: "name.html"
                },
                {
                    title: "orders.blades.customerOrder-item-detail.labels.sku",
                    colSpan: 2,
                    templateUrl: "sku.html"
                },
                {
                    templateUrl: "status.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.quantity",
                    templateUrl: "quantity.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.price",
                    templateUrl: "price.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.priceWithTax",
                    templateUrl: "priceWithTax.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.discount",
                    templateUrl: "discountAmount.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.discountWithTax",
                    templateUrl: "discountAmountWithTax.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.tax",
                    templateUrl: "taxTotal.html"
                },
                {
                    title: "orders.blades.customerOrder-items.labels.total",
                    templateUrl: "extendedPriceWithTax.html"
                }
            ];

            blade.refresh = function () {
                blade.isLoading = false;
            };

            blade.openStatusSettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html',
                    currentEntityId: 'OrderLineItem.Statuses',
                    isApiSave: true,
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.recalculate = function() {
                if (blade.formScope && blade.formScope.$valid && angular.isFunction(blade.recalculateFn)) {
                    blade.recalculateFn();
                }
            }

            blade.setForm = function (form) { blade.formScope = form; }

            blade.openItemDetail = function () {
                return productBladeResolver.open({
                    blade: blade,
                    item: blade.currentEntity,
                    orderId: blade.order.id
                });
            };

            $scope.$watch("blade.order", function (order) {
                blade.currentEntity = _.filter(order.items, function (item) { return item.id === blade.currentEntityId })[0];
            }, true);

            blade.refresh();
        }]);
