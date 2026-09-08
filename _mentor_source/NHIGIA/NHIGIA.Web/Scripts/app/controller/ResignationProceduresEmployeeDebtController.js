(function (angular) {
    "use strict";
    hrmApp.controller('ResignationProceduresEmployeeDebtController', [
        '$scope', 'ResignationProceduresEmployeeDebtService',
        function ($scope, ResignationProceduresEmployeeDebtService) {

            // Define
            $scope.IsSaveResignationProceduresEmployeeDebt = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.ResignationProceduresTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorNameOfTheDebtId = false;
            }

            // ShowPopupResignationProceduresEmployeeDebt
            $scope.ShowPopupResignationProceduresEmployeeDebt = function (e, id) {
                $scope.IsSaveResignationProceduresEmployeeDebt = false;
                $scope.CheckValidate = true;

                var valueClick = e;
                $("#btnSaveResignationProceduresEmployeeDebt").prop("disabled", false);
                $("#btnCloseResignationProceduresEmployeeDebt").prop("disabled", false);

                $scope.modelResignationProceduresEmployeeDebt = {
                    Id: 0,
                    ResignationProceduresId: STRING_EMPTY,
                    NameOfTheDebtId: STRING_EMPTY,
                    AmountOfMoney: 0,
                    FinishDay: STRING_EMPTY,
                    Accomplished: false
                };

                var windowResignationProceduresEmployeeDebt = $("#KenWindownResignationProceduresEmployeeDebt").kendoWindow({
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
                    case "ADD_RESIGNATIONPROCEDURESEMPLOYEEDEBT":
                        loadingPopUp();
                        windowResignationProceduresEmployeeDebt.title("Thêm mới");
                        windowResignationProceduresEmployeeDebt.open();
                        windowResignationProceduresEmployeeDebt.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_RESIGNATIONPROCEDURESEMPLOYEEDEBT":
                        loadingPopUp();
                        ResignationProceduresEmployeeDebtService.GetAllResignationProceduresEmployeeDebt(id, $scope.ResignationProceduresTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelResignationProceduresEmployeeDebt.Id = id;
                                $scope.modelResignationProceduresEmployeeDebt.ResignationProceduresId = $scope.ResignationProceduresTemp;
                                $scope.modelResignationProceduresEmployeeDebt.NameOfTheDebtId = response.data[0].NameOfTheDebtId;
                                $scope.modelResignationProceduresEmployeeDebt.AmountOfMoney = response.data[0].AmountOfMoney;
                                $scope.modelResignationProceduresEmployeeDebt.FinishDay = kendo.parseDate(response.data[0].FinishDay, DATE_FORMAT);
                                $scope.modelResignationProceduresEmployeeDebt.Accomplished = response.data[0].Accomplished;
                                windowResignationProceduresEmployeeDebt.title("Sửa dữ liệu");
                                windowResignationProceduresEmployeeDebt.open();
                                windowResignationProceduresEmployeeDebt.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_RESIGNATIONPROCEDURESEMPLOYEEDEBT":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelResignationProceduresEmployeeDebt);
                                data.Id = id;
                                ResignationProceduresEmployeeDebtService.DeleteResignationProceduresEmployeeDebt(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("EMPLOYEE_DEBT");
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

            // CloseResignationProceduresEmployeeDebt
            $scope.CloseResignationProceduresEmployeeDebt = function () {
                $scope.IsSaveResignationProceduresEmployeeDebt = false;
                onShowMessageValidate();
                $("#KenWindownResignationProceduresEmployeeDebt").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueResignationProceduresEmployeeDebt
            $scope.onChangeValueResignationProceduresEmployeeDebt = function (e) {
                switch (e) {
                    case "NameOfTheDebtId":
                        if ($scope.modelResignationProceduresEmployeeDebt.NameOfTheDebtId !== STRING_EMPTY) {
                            $scope.showHasErrorNameOfTheDebtId = false;
                        } else {
                            $scope.showHasErrorNameOfTheDebtId = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveResignationProceduresEmployeeDebt
            $scope.SaveResignationProceduresEmployeeDebt = function (form) {
                $scope.IsSaveResignationProceduresEmployeeDebt = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelResignationProceduresEmployeeDebt.NameOfTheDebtId || $scope.modelResignationProceduresEmployeeDebt.NameOfTheDebtId === STRING_EMPTY) {
                        $scope.showHasErrorNameOfTheDebtId = true;
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelResignationProceduresEmployeeDebt);
                data.ResignationProceduresId = $scope.ResignationProceduresTemp;
                data.FinishDay = kendo.parseDate($scope.modelResignationProceduresEmployeeDebt.FinishDay, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveResignationProceduresEmployeeDebt").prop("disabled", true);
                $("#btnCloseResignationProceduresEmployeeDebt").prop("disabled", true);
                ResignationProceduresEmployeeDebtService.SaveResignationProceduresEmployeeDebt(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownResignationProceduresEmployeeDebt").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("EMPLOYEE_DEBT");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownResignationProceduresEmployeeDebt").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };
        }]);

})(window.angular);
