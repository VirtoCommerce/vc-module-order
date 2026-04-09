angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.customerOrderItemConfigurationController', [
        '$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.blade;
            blade.title = 'orders.blades.customerOrder-item-configuration.title';
            blade.headIcon = 'fas fa-sliders';

            $scope.showProducts = function() {
                var newBlade = {
                    id: "сonfigurationProducts",
                    controller: 'virtoCommerce.orderModule.customerOrderItemConfigurationProductsController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-configuration-products.tpl.html',
                    currentEntity: blade.currentEntity,
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade.parentBlade);
            }

            $scope.showTexts = function () {
                var newBlade = {
                    id: "сonfigurationProducts",
                    controller: 'virtoCommerce.orderModule.customerOrderItemConfigurationTextsController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-configuration-texts.tpl.html',
                    currentEntity: blade.currentEntity,
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade.parentBlade);
            }

            $scope.showFiles = function () {
                var newBlade = {
                    id: "сonfigurationFiles",
                    controller: 'virtoCommerce.orderModule.customerOrderItemConfigurationFilesController',
                    template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/customerOrder-item-configuration-files.tpl.html',
                    currentEntity: blade.currentEntity,
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade.parentBlade);
            }

            blade.isLoading = false;
        }]);
