(function (angular) {
    "use strict";
    hrmApp.controller('EmployeesOnBusinessTripPaymentsController', [
        '$scope', 'EmployeesOnBusinessTripPaymentsService',
        function ($scope, EmployeesOnBusinessTripPaymentsService) {

            // Define
            $scope.IsSaveEmployeesOnBusinessTripPayments = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeesOnBusinessTripIdTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorExpensesEmployeesOnBusinessTripPayments = false;
            }

            // ShowPopupEmployeesOnBusinessTripPayments
            $scope.ShowPopupEmployeesOnBusinessTripPayments = function (e, id) {
                $scope.IsSaveEmployeesOnBusinessTripPayments = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveEmployeesOnBusinessTripPayments").prop("disabled", false);
                $("#btnCloseEmployeesOnBusinessTripPayments").prop("disabled", false);

                $scope.modelEmployeesOnBusinessTripPayments = {
                    Id: 0,
                    EmployeesOnBusinessTripId: STRING_EMPTY,
                    Expenses: STRING_EMPTY,
                    VoucherNumber: STRING_EMPTY,
                    DayVouchers: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    AmountSpent: 0,
                    Note: STRING_EMPTY
                };

                var windowEmployeesOnBusinessTripPayments = $("#KenWindownEmployeesOnBusinessTripPayments").kendoWindow({
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
                    case "ADD_EMPLOYEESONBUSINESSTRIPPAYMENTS":
                        loadingPopUp();
                        windowEmployeesOnBusinessTripPayments.title("Thêm mới");
                        windowEmployeesOnBusinessTripPayments.open();
                        windowEmployeesOnBusinessTripPayments.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_EMPLOYEESONBUSINESSTRIPPAYMENTS":
                        loadingPopUp();
                        EmployeesOnBusinessTripPaymentsService.GetAllEmployeesOnBusinessTripPayments(id, $scope.EmployeesOnBusinessTripIdTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelEmployeesOnBusinessTripPayments.Id = id;
                                $scope.modelEmployeesOnBusinessTripPayments.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                                $scope.modelEmployeesOnBusinessTripPayments.Expenses = response.data[0].Expenses;
                                $scope.modelEmployeesOnBusinessTripPayments.VoucherNumber = response.data[0].VoucherNumber;
                                $scope.modelEmployeesOnBusinessTripPayments.DayVouchers = kendo.parseDate(response.data[0].DayVouchers, DATE_FORMAT);
                                $scope.modelEmployeesOnBusinessTripPayments.AmountSpent = response.data[0].AmountSpent;
                                $scope.modelEmployeesOnBusinessTripPayments.Note = response.data[0].Note;
                                windowEmployeesOnBusinessTripPayments.title("Sửa dữ liệu");
                                windowEmployeesOnBusinessTripPayments.open();
                                windowEmployeesOnBusinessTripPayments.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_EMPLOYEESONBUSINESSTRIPPAYMENTS":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelEmployeesOnBusinessTripPayments);
                                data.Id = id;
                                data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                                EmployeesOnBusinessTripPaymentsService.DeleteEmployeesOnBusinessTripPayments(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("PAYMENTS");
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

            // CloseEmployeesOnBusinessTripPayments
            $scope.CloseEmployeesOnBusinessTripPayments = function () {
                $scope.IsSaveEmployeesOnBusinessTripPayments = false;
                onShowMessageValidate();
                $("#KenWindownEmployeesOnBusinessTripPayments").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueEmployeesOnBusinessTripPayments
            $scope.onChangeValueEmployeesOnBusinessTripPayments = function (e) {
                switch (e) {
                    case "Expenses":
                        if ($scope.modelEmployeesOnBusinessTripPayments.Expenses !== STRING_EMPTY) {
                            $scope.showHasErrorExpensesEmployeesOnBusinessTripPayments = false;
                        } else {
                            $scope.showHasErrorExpensesEmployeesOnBusinessTripPayments = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveEmployeesOnBusinessTripPayments
            $scope.SaveEmployeesOnBusinessTripPayments = function (form) {
                $scope.IsSaveEmployeesOnBusinessTripPayments = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelEmployeesOnBusinessTripPayments.Expenses || $scope.modelEmployeesOnBusinessTripPayments.Expenses === STRING_EMPTY) {
                        $scope.showHasErrorExpensesEmployeesOnBusinessTripPayments = true;
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelEmployeesOnBusinessTripPayments);
                data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                data.DayVouchers = kendo.parseDate($scope.modelEmployeesOnBusinessTripPayments.DayVouchers, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveEmployeesOnBusinessTripPayments").prop("disabled", true);
                $("#btnCloseEmployeesOnBusinessTripPayments").prop("disabled", true);
                EmployeesOnBusinessTripPaymentsService.SaveEmployeesOnBusinessTripPayments(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownEmployeesOnBusinessTripPayments").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("PAYMENTS");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownEmployeesOnBusinessTripPayments").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
