(function (angular) {
    "use strict";
    hrmApp.controller('WorkProgressInformationController', [
        '$scope', 'WorkProgressInformationService', 'EmployeeInformationService',
        function ($scope, WorkProgressInformationService, EmployeeInformationService) {

            // Define
            $scope.IsSaveWorkProgress = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorJobPositionIdWorkProgress = false;
                $scope.showHasErrorWorkUnitIdWorkProgress = false;
                $scope.showHasErrorWorkStatusIdWorkProgress = false;
            }

            // ShowPopupWorkProgress
            $scope.ShowPopupWorkProgress = function (e, id) {
                $scope.IsSaveWorkProgress = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveWorkProgress").prop("disabled", false);
                $("#btnCloseWorkProgress").prop("disabled", false);

                $scope.modelWorkProgress = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    StartDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    EndDate: STRING_EMPTY,
                    JobPositionId: STRING_EMPTY,
                    WorkUnitId: STRING_EMPTY,
                    WorkStatusId: STRING_EMPTY,
                    DirectManagementId: STRING_EMPTY,
                    IndirectManagementId: STRING_EMPTY,
                    DecisionNumber: STRING_EMPTY,
                    DecisionDate: STRING_EMPTY,
                    Note: STRING_EMPTY,
                    DirectManagementWorkProgressAutoComplete: STRING_EMPTY,
                    IndirectManagementIdWorkProgressAutoComplete: STRING_EMPTY
                };

                var windowWorkProgress = $("#KenWindownWorkProgress").kendoWindow({
                    actions: ["Close"],
                    draggable: true,
                    modal: true,
                    pinned: false,
                    position: {
                        top: 15
                    },
                    resizable: false,
                    width: "50%"
                }).data('kendoWindow');

                switch (valueClick) {
                    case "ADD_WORKPROGRESS":
                        loadingPopUp();
                        $scope.modelWorkProgress.JobPositionId = $scope.$parent.model.JobPositionId;
                        $scope.modelWorkProgress.WorkUnitId = $scope.$parent.model.WorkUnitId;
                        $scope.modelWorkProgress.WorkStatusId = $scope.$parent.model.WorkStatusId;
                        $scope.modelWorkProgress.DirectManagementId = $scope.$parent.model.DirectManagementId;
                        $scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete = $scope.$parent.model.DirectManagementAutoComplete;
                        $scope.modelWorkProgress.IndirectManagementId = $scope.$parent.model.IndirectManagementId;
                        $scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete = $scope.$parent.model.IndirectManagementAutoComplete;

                        windowWorkProgress.title("Thêm mới");
                        windowWorkProgress.open();
                        windowWorkProgress.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_WORKPROGRESS":
                        loadingPopUp();
                        WorkProgressInformationService.GetAllWorkProgressInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelWorkProgress.Id = id;
                                $scope.modelWorkProgress.EmployeeId = response.data[0].EmployeeId;
                                $scope.modelWorkProgress.StartDate = kendo.parseDate(response.data[0].StartDate, DATE_FORMAT);
                                $scope.modelWorkProgress.EndDate = response.data[0].EndDate;
                                $scope.modelWorkProgress.JobPositionId = response.data[0].JobPositionId;
                                $scope.modelWorkProgress.WorkUnitId = response.data[0].WorkUnitId;
                                $scope.modelWorkProgress.WorkStatusId = response.data[0].WorkStatusId;
                                $scope.modelWorkProgress.DirectManagementId = response.data[0].DirectManagementId;
                                $scope.modelWorkProgress.IndirectManagementId = response.data[0].IndirectManagementId;
                                $scope.modelWorkProgress.DecisionNumber = response.data[0].DecisionNumber;
                                $scope.modelWorkProgress.DecisionDate = kendo.parseDate(response.data[0].DecisionDate, DATE_FORMAT);
                                $scope.modelWorkProgress.Note = response.data[0].Note;
                                $scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete = response.data[0].DirectManagementName;
                                $scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete = response.data[0].IndirectManagementName;
                                windowWorkProgress.title("Sửa dữ liệu");
                                windowWorkProgress.open();
                                windowWorkProgress.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_WORKPROGRESS":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelWorkProgress);
                                data.Id = id;
                                data.EmployeeId = $scope.EmployeeTemp;
                                WorkProgressInformationService.DeleteWorkProgressInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("WORKPROGRESS");
                                        stopLoadingPopUp();
                                    } else {
                                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                        stopLoadingPopUp();
                                    }
                                });
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            // CloseWorkProgress
            $scope.CloseWorkProgress = function () {
                $scope.IsSaveWorkProgress = false;
                onShowMessageValidate();
                $("#KenWindownWorkProgress").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueWorkProgress
            $scope.onChangeValueWorkProgress = function (e) {
                switch (e) {
                    case "JobPositionIdWorkProgress":
                        if ($scope.modelWorkProgress.JobPositionId !== STRING_EMPTY) {
                            $scope.showHasErrorJobPositionIdWorkProgress = false;
                        } else {
                            $scope.showHasErrorJobPositionIdWorkProgress = true;
                        }
                        break;
                    case "WorkUnitIdWorkProgress":
                        if ($scope.modelWorkProgress.WorkUnitId !== STRING_EMPTY) {
                            $scope.showHasErrorWorkUnitIdWorkProgress = false;
                        } else {
                            $scope.showHasErrorWorkUnitIdWorkProgress = true;
                        }
                        break;
                    case "WorkStatusIdWorkProgress":
                        if ($scope.modelWorkProgress.WorkStatusId !== STRING_EMPTY) {
                            $scope.showHasErrorWorkStatusIdWorkProgress = false;
                        } else {
                            $scope.showHasErrorWorkStatusIdWorkProgress = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveWorkProgress
            $scope.SaveWorkProgress = function (form) {
                $scope.IsSaveWorkProgress = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelWorkProgress.JobPositionId || $scope.modelWorkProgress.JobPositionId === STRING_EMPTY) {
                        $scope.showHasErrorJobPositionIdWorkProgress = true;
                    }
                    if (!$scope.modelWorkProgress.WorkUnitId || $scope.modelWorkProgress.WorkUnitId === STRING_EMPTY) {
                        $scope.showHasErrorWorkUnitIdWorkProgress = true;
                    }
                    if (!$scope.modelWorkProgress.WorkStatusId || $scope.modelWorkProgress.WorkStatusId === STRING_EMPTY) {
                        $scope.showHasErrorWorkStatusIdWorkProgress = true;
                    }
                    if ($scope.showHasErrorJobPositionIdWorkProgress === true || $scope.showHasErrorWorkUnitIdWorkProgress === true || $scope.showHasErrorWorkStatusIdWorkProgress === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelWorkProgress);
                data.EmployeeId = $scope.EmployeeTemp;
                data.StartDate = kendo.parseDate($scope.modelWorkProgress.StartDate, DATE_FORMAT);
                data.DecisionDate = kendo.parseDate($scope.modelWorkProgress.DecisionDate, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveWorkProgress").prop("disabled", true);
                $("#btnCloseWorkProgress").prop("disabled", true);
                WorkProgressInformationService.SaveWorkProgressInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownWorkProgress").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("WORKPROGRESS");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownWorkProgress").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // Load datasource for DirectManagementId autocomplete box
            $scope.GetEmployeeForDirectManagementWorkProgress = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item DirectManagementId autocomplete box
            $scope.onSelectDirectManagementIdWorkProgress = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForDirectManagementWorkProgress.data()[index];
                $scope.modelWorkProgress.DirectManagementId = itemSelected.Id;
                $scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#DirectManagementIdWorkProgress").data("kendoAutoComplete").close();
                e.preventDefault();
            }

            // Event blur item DirectManagementId autocomplete box
            $scope.onChangeDirectManagementIdWorkProgress = function () {
                if ($scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete === $scope.$parent.model.DirectManagementAutoComplete) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForDirectManagementWorkProgress.data().forEach(function (item) {
                    if ($scope.modelWorkProgress.DirectManagementId === item.Id) {
                        $scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });

                if (!matchCode) {
                    $scope.modelWorkProgress.DirectManagementId = STRING_EMPTY;
                    $scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete = STRING_EMPTY;
                }
            };

            // onChangeDirectManagementIdValue
            $scope.onChangeDirectManagementIdWorkProgressValue = function () {
                if ($scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete === STRING_EMPTY) {
                    $scope.modelWorkProgress.DirectManagementId = STRING_EMPTY;
                    $scope.modelWorkProgress.DirectManagementWorkProgressAutoComplete = STRING_EMPTY;
                }
            };

            // Load datasource for IndirectManagementId autocomplete box
            $scope.GetEmployeeForIndirectManagementWorkProgress = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item IndirectManagementId autocomplete box
            $scope.onSelectIndirectManagementIdWorkProgress = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForIndirectManagementWorkProgress.data()[index];
                $scope.modelWorkProgress.IndirectManagementId = itemSelected.Id;
                $scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#IndirectManagementIdWorkProgress").data("kendoAutoComplete").close();
                e.preventDefault();
            }

            // Event blur item IndirectManagementId autocomplete box
            $scope.onChangeIndirectManagementIdWorkProgress = function () {
                if ($scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete === $scope.$parent.model.IndirectManagementAutoComplete) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForIndirectManagementWorkProgress.data().forEach(function (item) {
                    if ($scope.modelWorkProgress.IndirectManagementId === item.Id) {
                        $scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelWorkProgress.IndirectManagementId = STRING_EMPTY;
                    $scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete = STRING_EMPTY;
                }
            };

            // onChangeIndirectManagementIdValue
            $scope.onChangeIndirectManagementIdWorkProgressValue = function () {
                if ($scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete === STRING_EMPTY) {
                    $scope.modelWorkProgress.IndirectManagementId = STRING_EMPTY;
                    $scope.modelWorkProgress.IndirectManagementWorkProgressAutoComplete = STRING_EMPTY;
                }
            };


        }]);

})(window.angular);
