angular.module('virtoCommerce.orderModule')
    .controller('virtoCommerce.orderModule.uploadWorkflowController',
    ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.orderModule.workflows', 'FileUploader',
        function ($scope, bladeNavigationService, workflows, FileUploader) {
            var blade = $scope.blade;
            var _item, _file = {};

            blade.isLoading = false;
            blade.enabledWorkFlow = false;
            $scope.isUploadSuccess = false;
            $scope.hasWorkflow = false;
            $scope.hasFileChanged = false;
            $scope.hasStatusChanged = false;
            
            resetWorkflowData();
            // Initialize Json Uploader
            if (!$scope.uploader) {
                // create the uploader
                var uploader = $scope.uploader = new FileUploader({
                    scope: $scope,
                    headers: { Accept: 'application/json' },
                    url: 'api/platform/assets?folderUrl=OrganizationWorkflow/' + blade.currentEntity.id + '/' + new Date().getTime(),
                    method: 'POST',
                    autoUpload: false,
                    removeAfterUpload: true
                });

                uploader.onBeforeUploadItem = function (fileItem) {
                    blade.isLoading = true;
                };

                uploader.onSuccessItem = function (fileItem, asset, status, headers) {
                    _item = null;
                    _file = {
                        jsonPath: asset[0].relativeUrl,
                        workflowName: asset[0].name,
                        createdDate: new Date().toISOString()
                    };
                    $scope.isUploadSuccess = true;
                    $scope.hasFileChanged = true;
                    blade.isLoading = false;
                };

                uploader.onAfterAddingFile = function (item) {
                    _item = item;
                    $scope.jsonPath = item.file.name;
                    $scope.isUploadSuccess = false;
                    bladeNavigationService.setError(null, blade);
                };

                uploader.onErrorItem = function (item, response, status, headers) {
                    _file = {};
                    resetWorkflowData();
                    $scope.isUploadSuccess = false;
                    $scope.hasFileChanged = false;
                    bladeNavigationService.setError(item._file.name + ' failed: ' + (response.message ? response.message : status), blade);
                };

                $scope.uploadWorkflow = function () {
                    if (_item) uploader.uploadItem(_item);
                };
            }

            $scope.onStatusChanged = function () {
                if (blade.enabledWorkFlow !== blade.workflow.status) $scope.hasStatusChanged = true;
                else $scope.hasStatusChanged = false;
            };

            $scope.blade.toolbarCommands = [
                {
                    name: 'platform.commands.save',
                    icon: 'fa fa-save',
                    executeMethod: function () {
                        blade.isLoading = true;
                        var workflowParams = angular.extend({
                            organizationId: blade.currentEntity.id
                        }, _file, { status: blade.enabledWorkFlow });
                        // Save file information
                        workflows.updateWorkflow(workflowParams)
                            .$promise.then(function (workflow) {
                                workflow = workflow.data;
                                $scope.jsonPath = '';
                                $scope.hasWorkflow = true;
                                blade.enabledWorkFlow = workflow.status;
                                blade.workflow = workflow;
                                $scope.hasStatusChanged = false;
                                $scope.hasFileChanged = false;
                                $scope.isUploadSuccess = false;
                                blade.isLoading = false;
                            }, function (response) {
                                bladeNavigationService.setError(response, blade);
                            });
                    },
                    canExecuteMethod: function () {
                        return $scope.hasStatusChanged || $scope.hasFileChanged;
                    },
                    permission: 'workflow:upload'
                }
            ];

            function resetWorkflowData() {
                if (typeof blade.workflow !== 'undefined') {
                    $scope.hasWorkflow = true;
                    blade.enabledWorkFlow = blade.workflow.status;
                }
            }
}]);
