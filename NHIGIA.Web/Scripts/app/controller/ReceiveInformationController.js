(function (angular) {
    "use strict";
    hrmApp.controller('ReceiveInformationController', [
        '$scope', 'ReceiveInformationService',
        function ($scope, ReceiveInformationService) {

            // Define
            $scope.IsSaveReceive = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorNameOfProcedure = false;
            }

            // ShowPopupReceive
            $scope.ShowPopupReceive = function (e, id) {
                $scope.IsSaveReceive = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveReceive").prop("disabled", false);
                $("#btnCloseReceive").prop("disabled", false);

                $scope.modelReceive = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    NameOfProcedure: STRING_EMPTY,
                    ProcedureGroupReceiveId: STRING_EMPTY,
                    Accomplished: false,
                    FinishDay: STRING_EMPTY
                };

                var windowReceive = $("#KenWindownReceive").kendoWindow({
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
                    case "ADD_RECEIVE":
                        loadingPopUp();
                        windowReceive.title("Thêm mới");
                        windowReceive.open();
                        windowReceive.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_RECEIVE":
                        loadingPopUp();
                        ReceiveInformationService.GetAllReceiveInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelReceive.Id = id;
                                $scope.modelReceive.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelReceive.NameOfProcedure = response.data[0].NameOfProcedure;
                                $scope.modelReceive.ProcedureGroupReceiveId = response.data[0].ProcedureGroupReceiveId;
                                $scope.modelReceive.Accomplished = response.data[0].Accomplished;
                                $scope.modelReceive.FinishDay = kendo.parseDate(response.data[0].FinishDay, DATE_FORMAT);
                                windowReceive.title("Sửa dữ liệu");
                                windowReceive.open();
                                windowReceive.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_RECEIVE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelReceive);
                                data.Id = id;
                                ReceiveInformationService.DeleteReceiveInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("RECEIVE");
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

            // CloseReceive
            $scope.CloseReceive = function () {
                $scope.IsSaveReceive = false;
                onShowMessageValidate();
                $("#KenWindownReceive").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueReceive
            $scope.onChangeValueReceive = function (e) {
                switch (e) {
                    case "NameOfProcedure":
                        if ($scope.modelReceive.NameOfProcedure !== STRING_EMPTY) {
                            $scope.showHasErrorNameOfProcedure = false;
                        } else {
                            $scope.showHasErrorNameOfProcedure = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveReceive
            $scope.SaveReceive = function (form) {
                $scope.IsSaveReceive = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelReceive.NameOfProcedure || $scope.modelReceive.NameOfProcedure === STRING_EMPTY) {
                        $scope.showHasErrorNameOfProcedure = true;
                    }
                    if ($scope.showHasErrorNameOfProcedure === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelReceive);
                data.EmployeeId = $scope.EmployeeTemp;
                data.FinishDay = kendo.parseDate($scope.modelReceive.FinishDay, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveReceive").prop("disabled", true);
                $("#btnCloseReceive").prop("disabled", true);
                ReceiveInformationService.SaveReceiveInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownReceive").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("RECEIVE");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownReceive").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // onChangeAccomplished
            $scope.onChangeAccomplished = function () {
                $scope.modelReceive.FinishDay = STRING_EMPTY;
            };
        }]);

})(window.angular);
