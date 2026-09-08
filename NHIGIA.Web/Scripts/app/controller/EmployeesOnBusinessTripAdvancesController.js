(function (angular) {
    "use strict";
    hrmApp.controller('EmployeesOnBusinessTripAdvancesController', [
        '$scope', 'EmployeesOnBusinessTripAdvancesService',
        function ($scope, EmployeesOnBusinessTripAdvancesService) {

            // Define
            $scope.IsSaveEmployeesOnBusinessTripAdvances = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeesOnBusinessTripIdTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorExpenses = false;
            }

            // ShowPopupEmployeesOnBusinessTripAdvances
            $scope.ShowPopupEmployeesOnBusinessTripAdvances = function (e, id) {
                $scope.IsSaveEmployeesOnBusinessTripAdvances = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveEmployeesOnBusinessTripAdvances").prop("disabled", false);
                $("#btnCloseEmployeesOnBusinessTripAdvances").prop("disabled", false);

                $scope.modelEmployeesOnBusinessTripAdvances = {
                    Id: 0,
                    EmployeesOnBusinessTripId: STRING_EMPTY,
                    Expenses: STRING_EMPTY,
                    Unit: STRING_EMPTY,
                    Amount: STRING_EMPTY,
                    UnitPrice: 0,
                    Money: 0,
                    Note: STRING_EMPTY
                };

                var windowEmployeesOnBusinessTripAdvances = $("#KenWindownEmployeesOnBusinessTripAdvances").kendoWindow({
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
                    case "ADD_EMPLOYEESONBUSINESSTRIPADVANCES":
                        loadingPopUp();
                        windowEmployeesOnBusinessTripAdvances.title("Thêm mới");
                        windowEmployeesOnBusinessTripAdvances.open();
                        windowEmployeesOnBusinessTripAdvances.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_EMPLOYEESONBUSINESSTRIPADVANCES":
                        loadingPopUp();
                        EmployeesOnBusinessTripAdvancesService.GetAllEmployeesOnBusinessTripAdvances(id, $scope.EmployeesOnBusinessTripIdTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelEmployeesOnBusinessTripAdvances.Id = id;
                                $scope.modelEmployeesOnBusinessTripAdvances.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                                $scope.modelEmployeesOnBusinessTripAdvances.Expenses = response.data[0].Expenses;
                                $scope.modelEmployeesOnBusinessTripAdvances.Unit = response.data[0].Unit;
                                $scope.modelEmployeesOnBusinessTripAdvances.Amount = response.data[0].Amount;
                                $scope.modelEmployeesOnBusinessTripAdvances.UnitPrice = response.data[0].UnitPrice;
                                $scope.modelEmployeesOnBusinessTripAdvances.Money = response.data[0].Money;
                                $scope.modelEmployeesOnBusinessTripAdvances.Note = response.data[0].Note;
                                windowEmployeesOnBusinessTripAdvances.title("Sửa dữ liệu");
                                windowEmployeesOnBusinessTripAdvances.open();
                                windowEmployeesOnBusinessTripAdvances.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_EMPLOYEESONBUSINESSTRIPADVANCES":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelEmployeesOnBusinessTripAdvances);
                                data.Id = id;
                                data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                                EmployeesOnBusinessTripAdvancesService.DeleteEmployeesOnBusinessTripAdvances(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("ADVANCES");
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

            // CloseEmployeesOnBusinessTripAdvances
            $scope.CloseEmployeesOnBusinessTripAdvances = function () {
                $scope.IsSaveEmployeesOnBusinessTripAdvances = false;
                onShowMessageValidate();
                $("#KenWindownEmployeesOnBusinessTripAdvances").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueEmployeesOnBusinessTripAdvances
            $scope.onChangeValueEmployeesOnBusinessTripAdvances = function (e) {
                switch (e) {
                    case "Expenses":
                        if ($scope.modelEmployeesOnBusinessTripAdvances.Expenses !== STRING_EMPTY) {
                            $scope.showHasErrorExpenses = false;
                        } else {
                            $scope.showHasErrorExpenses = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveEmployeesOnBusinessTripAdvances
            $scope.SaveEmployeesOnBusinessTripAdvances = function (form) {
                $scope.IsSaveEmployeesOnBusinessTripAdvances = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelEmployeesOnBusinessTripAdvances.Expenses || $scope.modelEmployeesOnBusinessTripAdvances.Expenses === STRING_EMPTY) {
                        $scope.showHasErrorExpenses = true;
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelEmployeesOnBusinessTripAdvances);
                data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;

                loadingPopUp();
                $("#btnSaveEmployeesOnBusinessTripAdvances").prop("disabled", true);
                $("#btnCloseEmployeesOnBusinessTripAdvances").prop("disabled", true);
                EmployeesOnBusinessTripAdvancesService.SaveEmployeesOnBusinessTripAdvances(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownEmployeesOnBusinessTripAdvances").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("ADVANCES");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownEmployeesOnBusinessTripAdvances").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
