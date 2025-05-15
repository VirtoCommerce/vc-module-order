angular.module('virtoCommerce.orderModule')
.controller('virtoCommerce.orderModule.customerOrderAddressWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
	$scope.operation = {};
	$scope.openAddressesBlade = function () {
		var newBlade = {
			id: 'orderOperationAddresses',
			title: 'orders.widgets.customerOrder-address.blade-title',
			currentEntities: $scope.operation.addresses,
			controller: 'virtoCommerce.orderModule.addressListController',
			template: 'Modules/$(VirtoCommerce.Orders)/Scripts/blades/addresses/address-list.tpl.html'
		};
		bladeNavigationService.showBlade(newBlade, $scope.blade);
	};
	$scope.$watch('widget.blade.customerOrder', function (operation) {
		$scope.operation = operation;
	});
}]);
