(function (angular) {
    "use strict";
    hrmApp.controller('QuitInformationController', [
        '$scope', 'QuitInformationService',
        function ($scope, QuitInformationService) {

            // Define
            $scope.IsSaveQuit = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorNameOfProcedure = false;
            }

            // ShowPopupQuit
            $scope.ShowPopupQuit = function (e, id) {
                $scope.IsSaveQuit = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveQuit").prop("disabled", false);
                $("#btnCloseQuit").prop("disabled", false);

                $scope.modelQuit = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    NameOfProcedure: STRING_EMPTY,
                    ProcedureGroupQuitId: STRING_EMPTY,
                    Accomplished: false,
                    FinishDay: STRING_EMPTY
                };

                var windowQuit = $("#KenWindownQuit").kendoWindow({
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
                    case "ADD_QUIT":
                        loadingPopUp();
                        windowQuit.title("Thêm mới");
                        windowQuit.open();
                        windowQuit.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_QUIT":
                        loadingPopUp();
                        QuitInformationService.GetAllQuitInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelQuit.Id = id;
                                $scope.modelQuit.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelQuit.NameOfProcedure = response.data[0].NameOfProcedure;
                                $scope.modelQuit.ProcedureGroupQuitId = response.data[0].ProcedureGroupQuitId;
                                $scope.modelQuit.Accomplished = response.data[0].Accomplished;
                                $scope.modelQuit.FinishDay = kendo.parseDate(response.data[0].FinishDay, DATE_FORMAT);
                                windowQuit.title("Sửa dữ liệu");
                                windowQuit.open();
                                windowQuit.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_QUIT":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelQuit);
                                data.Id = id;
                                QuitInformationService.DeleteQuitInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("QUIT");
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

            // CloseQuit
            $scope.CloseQuit = function () {
                $scope.IsSaveQuit = false;
                onShowMessageValidate();
                $("#KenWindownQuit").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueQuit
            $scope.onChangeValueQuit = function (e) {
                switch (e) {
                    case "NameOfProcedureQuit":
                        if ($scope.modelQuit.NameOfProcedure !== STRING_EMPTY) {
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

            // SaveQuit
            $scope.SaveQuit = function (form) {
                $scope.IsSaveQuit = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelQuit.NameOfProcedure || $scope.modelQuit.NameOfProcedure === STRING_EMPTY) {
                        $scope.showHasErrorNameOfProcedure = true;
                    }
                    if ($scope.showHasErrorNameOfProcedure === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelQuit);
                data.EmployeeId = $scope.EmployeeTemp;
                data.FinishDay = kendo.parseDate($scope.modelQuit.FinishDay, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveQuit").prop("disabled", true);
                $("#btnCloseQuit").prop("disabled", true);
                QuitInformationService.SaveQuitInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownQuit").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("QUIT");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownQuit").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // onChangeAccomplished
            $scope.onChangeAccomplished = function () {
                $scope.modelQuit.FinishDay = STRING_EMPTY;
            };
        }]);

})(window.angular);
