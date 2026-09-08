(function (angular) {
    "use strict";
    hrmApp.controller('TrainingProcessInformationController', [
        '$scope', 'TrainingProcessInformationService',
        function ($scope, TrainingProcessInformationService) {

            // Define
            $scope.IsSaveTrainingProcess = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.CheckEndDate = false;
            $scope.MessageCheckEndDate = "Ngày kết thúc phải lớn hơn hoặc bằng Ngày bắt đầu";

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorTrainingProcessName = false;
            }

            // ShowPopupTrainingProcess
            $scope.ShowPopupTrainingProcess = function (e, id) {
                $scope.IsSaveTrainingProcess = false;
                $scope.CheckValidate = true;
                $scope.CheckEndDate = false;
                var valueClick = e;
                $("#btnSaveTrainingProcess").prop("disabled", false);
                $("#btnCloseTrainingProcess").prop("disabled", false);

                $scope.modelTrainingProcess = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    TrainingProcessCode: STRING_EMPTY,
                    TrainingProcessName: STRING_EMPTY,
                    StartDay: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    EndDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    Purpose: STRING_EMPTY
                };

                var windowTrainingProcess = $("#KenWindownTrainingProcess").kendoWindow({
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
                    case "ADD_TRAININGPROCESS":
                        loadingPopUp();
                        windowTrainingProcess.title("Thêm mới");
                        windowTrainingProcess.open();
                        windowTrainingProcess.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_TRAININGPROCESS":
                        loadingPopUp();
                        TrainingProcessInformationService.GetAllTrainingProcessInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelTrainingProcess.Id = id;
                                $scope.modelTrainingProcess.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelTrainingProcess.TrainingProcessCode = response.data[0].TrainingProcessCode;
                                $scope.modelTrainingProcess.TrainingProcessName = response.data[0].TrainingProcessName;
                                $scope.modelTrainingProcess.StartDay = kendo.parseDate(response.data[0].StartDay, DATE_FORMAT);
                                $scope.modelTrainingProcess.EndDate = kendo.parseDate(response.data[0].EndDate, DATE_FORMAT);
                                $scope.modelTrainingProcess.Purpose = response.data[0].Purpose;
                                windowTrainingProcess.title("Sửa dữ liệu");
                                windowTrainingProcess.open();
                                windowTrainingProcess.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_TRAININGPROCESS":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelTrainingProcess);
                                data.Id = id;
                                TrainingProcessInformationService.DeleteTrainingProcessInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("TRAININGPROCESS");
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

            // CloseTrainingProcess
            $scope.CloseTrainingProcess = function () {
                $scope.IsSaveTrainingProcess = false;
                onShowMessageValidate();
                $("#KenWindownTrainingProcess").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueTrainingProcess
            $scope.onChangeValueTrainingProcess = function (e) {
                switch (e) {
                    case "TrainingProcessName":
                        if ($scope.modelTrainingProcess.TrainingProcessName !== STRING_EMPTY) {
                            $scope.showHasErrorTrainingProcessName = false;
                        } else {
                            $scope.showHasErrorTrainingProcessName = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveTrainingProcess
            $scope.SaveTrainingProcess = function (form) {
                $scope.IsSaveTrainingProcess = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelTrainingProcess.TrainingProcessName || $scope.modelTrainingProcess.TrainingProcessName === STRING_EMPTY) {
                        $scope.showHasErrorTrainingProcessName = true;
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelTrainingProcess);
                data.EmployeeId = $scope.EmployeeTemp;
                data.StartDay = kendo.parseDate($scope.modelTrainingProcess.StartDay, DATE_FORMAT);
                data.EndDate = kendo.parseDate($scope.modelTrainingProcess.EndDate, DATE_FORMAT);

                // Check StartDate < Enđate
                if (data.StartDay !== null && data.EndDate !== null && data.StartDay.getTime() > data.EndDate.getTime()) {
                    $scope.CheckEndDate = true;
                    return;
                } else {
                    $scope.CheckEndDate = false;
                }

                loadingPopUp();
                $("#btnSaveTrainingProcess").prop("disabled", true);
                $("#btnCloseTrainingProcess").prop("disabled", true);
                TrainingProcessInformationService.SaveTrainingProcessInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownTrainingProcess").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("TRAININGPROCESS");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownTrainingProcess").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
