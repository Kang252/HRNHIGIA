(function (angular) {
    "use strict";
    hrmApp.controller('SalaryHistoryInformationController', [
        '$scope', 'SalaryHistoryInformationService',
        function ($scope, SalaryHistoryInformationService) {

            // Define
            $scope.IsSaveSalaryHistory = false;
            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // ShowPopupSalaryHistory
            $scope.ShowPopupSalaryHistory = function (e, id) {
                $scope.IsSaveSalaryHistory = false;
                var valueClick = e;
                $("#btnSaveSalaryHistory").prop("disabled", false);
                $("#btnCloseSalaryHistory").prop("disabled", false);

                $scope.modelSalaryHistory = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    JobPositionId: STRING_EMPTY,
                    DateOfChange: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    BasicSalary: 0,
                    InsurancePremiums: 0,
                    JoinInsurance: false,
                    Explain: STRING_EMPTY
                };

                var windowSalaryHistory = $("#KenWindownSalaryHistory").kendoWindow({
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
                    case "ADD_SALARYHISTORY":
                        loadingPopUp();
                        $scope.modelSalaryHistory.JobPositionId = $scope.$parent.model.JobPositionId;
                        $scope.modelSalaryHistory.BasicSalary = $scope.$parent.model.BasicSalary;
                        $scope.modelSalaryHistory.InsurancePremiums = $scope.$parent.model.InsurancePremiums;
                        if ($scope.$parent.model.DateOfInsurance && $scope.$parent.model.DateOfInsurance !== STRING_EMPTY) {
                            $scope.modelSalaryHistory.JoinInsurance = true;
                        }
                        windowSalaryHistory.title("Thêm mới");
                        windowSalaryHistory.open();
                        windowSalaryHistory.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_SALARYHISTORY":
                        loadingPopUp();
                        SalaryHistoryInformationService.GetAllSalaryHistoryInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelSalaryHistory.Id = id;
                                $scope.modelSalaryHistory.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelSalaryHistory.JobPositionId = response.data[0].JobPositionId;
                                $scope.modelSalaryHistory.DateOfChange = kendo.parseDate(response.data[0].DateOfChange, DATE_FORMAT);
                                $scope.modelSalaryHistory.BasicSalary = response.data[0].BasicSalary;
                                $scope.modelSalaryHistory.InsurancePremiums = response.data[0].InsurancePremiums;
                                $scope.modelSalaryHistory.JoinInsurance = response.data[0].JoinInsurance;
                                $scope.modelSalaryHistory.Explain = response.data[0].Explain;
                                windowSalaryHistory.title("Sửa dữ liệu");
                                windowSalaryHistory.open();
                                windowSalaryHistory.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_SALARYHISTORY":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelSalaryHistory);
                                data.Id = id;
                                data.EmployeeId = $scope.EmployeeTemp;
                                SalaryHistoryInformationService.DeleteSalaryHistoryInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("SALARYHISTORY");
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

            // CloseSalaryHistory
            $scope.CloseSalaryHistory = function () {
                $scope.IsSaveSalaryHistory = false;
                $("#KenWindownSalaryHistory").closest(".k-window-content").data("kendoWindow").close();
            };

            // SaveSalaryHistory
            $scope.SaveSalaryHistory = function (form) {
                $scope.IsSaveSalaryHistory = true;

                var data = angular.copy($scope.modelSalaryHistory);
                data.EmployeeId = $scope.EmployeeTemp;
                data.DateOfChange = kendo.parseDate($scope.modelSalaryHistory.DateOfChange, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveSalaryHistory").prop("disabled", true);
                $("#btnCloseSalaryHistory").prop("disabled", true);
                SalaryHistoryInformationService.SaveSalaryHistoryInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownSalaryHistory").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("SALARYHISTORY");
                        stopLoadingPopUp();
                    } else if (response.data.status === 400) {
                        $("#btnSaveSalaryHistory").prop("disabled", false);
                        $("#btnCloseSalaryHistory").prop("disabled", false);
                        window.alert(response.data.message);
                        stopLoadingPopUp();
                    }
                    else {
                        $("#KenWindownSalaryHistory").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };
        }]);

})(window.angular);
