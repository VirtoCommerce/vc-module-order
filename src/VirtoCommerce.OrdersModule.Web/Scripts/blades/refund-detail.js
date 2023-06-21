angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.refundDetailController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'platformWebApp.settings',
        'virtoCommerce.orderModule.statusTranslationService',
        'platformWebApp.authService',
        'virtoCommerce.paymentModule.paymentMethods',
        'virtoCommerce.customerModule.members',
    function ($scope, bladeNavigationService, settings, statusTranslationService, authService, paymentMethods, members) {
        var blade = $scope.blade;
        blade.isVisiblePrices = authService.checkPermission('order:read_prices');

        blade.title = 'orders.blades.refund-details.title';
        blade.titleValues = { number: blade.currentEntity.number };
        blade.subtitle = 'orders.blades.refund-details.subtitle';

        blade.realOperationsCollection = _.flatten(_.pluck(blade.customerOrder.inPayments, 'refunds'));

        paymentMethods.search({storeId: blade.customerOrder.storeId}, function (data) {
                blade.paymentMethods = data.results;
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
        });

        settings.getValues({ id: 'Refund.Status' }, translateBladeStatuses);
        blade.openStatusSettingManagement = function () {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                currentEntityId: 'Refund.Status',
                parentRefresh: translateBladeStatuses,
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        function translateBladeStatuses(data) {
            blade.statuses = statusTranslationService.translateStatuses(data, 'Refund');
        }

        blade.customInitialize = function () {
            blade.isLocked = false;
        };

        blade.setEntityStatus = function (status) {
            blade.currentEntity.status = status;
        };

        blade.fetchVendors = function (criteria) {
            return members.search(criteria);
        }

        blade.openVendorsManagement = function () {
            var newBlade = {
                id: 'vendorList',
                currentEntity: { id: null },
                controller: 'virtoCommerce.customerModule.memberListController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
