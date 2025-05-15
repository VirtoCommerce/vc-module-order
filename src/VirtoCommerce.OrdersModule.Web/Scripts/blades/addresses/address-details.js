angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.addressDetailsController',
        ['$scope', '$filter', 'platformWebApp.common.countries', 'platformWebApp.dialogService', 'platformWebApp.metaFormsService', 'platformWebApp.bladeNavigationService',
            function ($scope, $filter, countries, dialogService, metaFormsService, bladeNavigationService) {
                const blade = $scope.blade;

                blade.addressTypesDict = {
                    billing: 'Billing',
                    shipping: 'Shipping',
                    billingAndShipping: 'BillingAndShipping'
                }

                blade.addressTypes = [blade.addressTypesDict.billing, blade.addressTypesDict.shipping, blade.addressTypesDict.billingAndShipping];
                blade.metaFields = blade.metaFields && blade.metaFields.length ? blade.metaFields : metaFormsService.getMetaFields('OrderAddressDetails');

                const oldMetafiels = metaFormsService.getMetaFields('addressDetails');

                if (oldMetafiels && oldMetafiels.length) {
                    _.forEach(oldMetafiels, function (oldMetafield) {
                        const existingField = _.find(blade.metaFields, function (currentMetafield) {
                            return currentMetafield.name === oldMetafield.name || currentMetafield.templateUrl === oldMetafield.templateUrl;
                        })
                        if (!existingField) {
                            blade.metaFields.push(oldMetafield);
                        }
                    });
                }

                if (blade.currentEntity.isNew) {
                    blade.currentEntity.addressType = blade.addressTypesDict.shipping;
                }

                blade.origEntity = blade.currentEntity;
                blade.currentEntity = angular.copy(blade.origEntity);
                blade.countries = countries.query();

                blade.countries.$promise.then(
                    (allCountries) => {
                        $scope.$watch('blade.currentEntity.countryCode', (countryCode, old) => {
                            if (countryCode) {
                                const country = _.findWhere(allCountries, { id: countryCode });
                                if (country) {
                                    blade.currentEntity.countryName = country.name;

                                    if (countryCode !== old) {
                                        blade.currentEntity.regionName = undefined;
                                        currentRegions = [];
                                    }

                                    if (country.regions) {
                                        currentRegions = country.regions;
                                    } else {
                                        countries.queryRegions(countryCode).$promise.then((regions) => {
                                            country.regions = regions;
                                            currentRegions.push(...regions);
                                        });
                                    }
                                }
                            }
                            console.log(blade.currentEntity);
                            console.log(blade.origEntity);
                        });
                    }
                );

                let currentRegions = [];
                if (blade.currentEntity.regionName && !blade.currentEntity.regionId) {
                    addRegion(currentRegions, blade.currentEntity.regionName);
                }

                blade.getRegions = function (search) {
                    var results = currentRegions;

                    if (search && search.length > 1) {
                        let filtered = $filter('filter')(results, search);

                        if (!_.some(filtered)) {
                            addRegion(results, search);
                        } else if (filtered.length > 1) { // remove other (added) records
                            filtered = _.filter(filtered, (x) => !x.id && x.displayName.length > search.length);

                            for (const x of filtered) {
                                results.splice(_.indexOf(results, x), 1);
                            }
                        }
                    }

                    return results;
                };

                function addRegion(regions, name) {
                    regions.unshift({ name: name, displayName: name });
                }

                $scope.$watch('blade.currentEntity.regionName', function (regionName, old) {
                    if (regionName === old) {
                        return;
                    }

                    let newId = null;

                    if (regionName) {
                        const region = _.findWhere(currentRegions, { name: regionName });

                        if (region) {
                            newId = region.id;
                        }
                    }

                    blade.currentEntity.regionId = newId;
                });

                blade.toolbarCommands = [
                    {
                        name: 'platform.commands.save', icon: 'fas fa-save',
                        executeMethod: function () {
                            if (blade.confirmChangesFn) {
                                blade.confirmChangesFn(blade.currentEntity);
                            }

                            angular.copy(blade.currentEntity, blade.origEntity);
                            $scope.bladeClose();
                        },
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    },
                    {
                        name: 'platform.commands.reset', icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.origEntity, blade.currentEntity);
                        },
                        canExecuteMethod: isDirty
                    },
                    {
                        name: 'platform.commands.delete', icon: 'fas fa-trash-alt',
                        executeMethod: deleteEntry,
                        canExecuteMethod: function () {
                            return !blade.currentEntity.isNew;
                        }
                    }
                ];

                blade.isLoading = false;

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(
                        isDirty(),
                        canSave(),
                        blade,
                        $scope.saveChanges,
                        closeCallback,
                        'orders.dialogs.address-save.title',
                        'orders.dialogs.address-save.message'
                    );
                };

                $scope.setForm = function (form) {
                    $scope.formScope = form;
                };

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.origEntity);
                }

                function canSave() {
                    return isDirty() && $scope.formScope && $scope.formScope.$valid;
                }

                function deleteEntry() {
                    const dialog = {
                        id: 'confirmDelete',
                        title: 'orders.dialogs.address-delete.title',
                        message: 'orders.dialogs.address-delete.message',
                        callback: function (remove) {
                            if (remove) {
                                if (blade.deleteFn) {
                                    blade.deleteFn(blade.currentEntity);
                                }
                                $scope.bladeClose();
                                if (blade.numberOfAddresses(blade.origEntity.addressType) === 1 && blade.currentEntity.addressType !== blade.addressTypesDict.billingAndShipping) {
                                    blade.searchSecondAddress(blade.origEntity.addressType, blade.currentEntity.name);
                                }
                            }
                        }
                    }
                    dialogService.showConfirmationDialog(dialog);
                }
            }
        ]);
