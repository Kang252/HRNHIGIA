(function (angular) {
    "use strict";
    hrmApp.controller('AllowanceInformationController', [
        '$scope', 'AllowanceInformationService',
        function ($scope, AllowanceInformationService) {

            // Define
            $scope.IsSaveAllowance = false;
            $scope.CheckValidate = false;

            $scope.CheckToDate = false;
            $scope.MessageCheckToDate = "Đến ngày phải lớn hơn hoặc bằng với Từ ngày";

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorAllowanceType = false;
            }

            // ShowPopupAllowance
            $scope.ShowPopupAllowance = function (e, id) {
                $scope.IsSaveAllowance = false;
                $scope.CheckValidate = true;
                $scope.CheckToDate = false;
                var valueClick = e;
                $("#btnSaveAllowance").prop("disabled", false);
                $("#btnCloseAllowance").prop("disabled", false);

                $scope.modelAllowance = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    AllowanceTypeId: STRING_EMPTY,
                    Price: 0,
                    StartDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ToDate: STRING_EMPTY
                };

                var windowAllowance = $("#KenWindownAllowance").kendoWindow({
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
                    case "ADD_ALLOWANCE":
                        loadingPopUp();
                        windowAllowance.title("Thêm mới");
                        windowAllowance.open();
                        windowAllowance.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_ALLOWANCE":
                        loadingPopUp();
                        AllowanceInformationService.GetAllAllowanceInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelAllowance.Id = id;
                                $scope.modelAllowance.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelAllowance.AllowanceTypeId = response.data[0].AllowanceTypeId;
                                $scope.modelAllowance.Price = response.data[0].Price;
                                $scope.modelAllowance.StartDate = kendo.parseDate(response.data[0].StartDate, DATE_FORMAT);
                                $scope.modelAllowance.ToDate = kendo.parseDate(response.data[0].ToDate, DATE_FORMAT);
                                windowAllowance.title("Sửa dữ liệu");
                                windowAllowance.open();
                                windowAllowance.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_ALLOWANCE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelAllowance);
                                data.Id = id;
                                AllowanceInformationService.DeleteAllowanceInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("ALLOWANCE");
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

            // CloseAllowance
            $scope.CloseAllowance = function () {
                $scope.IsSaveAllowance = false;
                onShowMessageValidate();
                $("#KenWindownAllowance").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueAllowance
            $scope.onChangeValueAllowance = function (e) {
                switch (e) {
                    case "AllowanceTypeId":
                        if ($scope.modelAllowance.AllowanceTypeId !== STRING_EMPTY) {
                            $scope.showHasErrorAllowanceType = false;
                        } else {
                            $scope.showHasErrorAllowanceType = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveAllowance
            $scope.SaveAllowance = function (form) {
                $scope.IsSaveAllowance = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelAllowance.AllowanceTypeId || $scope.modelAllowance.AllowanceTypeId === STRING_EMPTY) {
                        $scope.showHasErrorAllowanceType = true;
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelAllowance);
                data.EmployeeId = $scope.EmployeeTemp;
                data.StartDate = kendo.parseDate($scope.modelAllowance.StartDate, DATE_FORMAT);
                data.ToDate = kendo.parseDate($scope.modelAllowance.ToDate, DATE_FORMAT);

                // Check validate StartDate < ToDate
                if (data.StartDate != null && data.ToDate != null && data.StartDate.getTime() > data.ToDate.getTime()) {
                    $scope.CheckToDate = true;
                    return;
                }
                else {
                    $scope.CheckToDate = false;
                }

                loadingPopUp();
                $("#btnSaveAllowance").prop("disabled", true);
                $("#btnCloseAllowance").prop("disabled", true);
                AllowanceInformationService.SaveAllowanceInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownAllowance").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("ALLOWANCE");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownAllowance").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };
        }]);

})(window.angular);
