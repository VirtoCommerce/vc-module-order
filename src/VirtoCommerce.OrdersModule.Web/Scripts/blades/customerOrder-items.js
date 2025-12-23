angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemsController', [
        '$scope', '$translate',
        'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        function ($scope, $translate, bladeNavigationService, authService) {
            var blade = $scope.blade;
            blade.updatePermission = 'order:update';
            blade.isVisiblePrices = authService.checkPermission('order:read_prices');

            $translate('orders.blades.customerOrder-detail.title', { customer: blade.currentEntity.customerName }).then(function (result) {
                blade.title = 'orders.widgets.customerOrder-items.blade-title';
                blade.titleValues = { title: result };
                blade.subtitle = 'orders.widgets.customerOrder-items.blade-subtitle';
            });

            blade.refresh = function (entity) {
                if (entity != null) {
                    blade.currentEntity = entity;
                }
                blade.isLoading = false;
                blade.selectedAll = false;
                blade.hasItems = blade.currentEntity && blade.currentEntity.items && blade.currentEntity.items.length > 0;
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

            $scope.openLineItemDetail = function (item, index) {
                blade.selectedNodeId = index;
                var newBlade = {
                    id: "listLineItemDetail",
                    controller: 'virtoCommerce.orderModule.customerOrderItemDetailController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-detail.tpl.html',
                    title: item.name,
                    currentEntityId: item.id,
                    order: blade.currentEntity,
                    recalculateFn: blade.recalculateFn,
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade);
            };

            $scope.checkAll = function (selected) {
                angular.forEach(blade.currentEntity.items, function (item) {
                    item.selected = selected;
                });
            };

            $scope.updateSelectedAll = function () {
                blade.selectedAll = _.every(blade.currentEntity.items, item => item.selected);
            };

            blade.refresh();
        }]);
