(function (angular) {
    "use strict";
    hrmApp.controller('EmployeesOnBusinessTripRevenueEstimatesController', [
        '$scope', 'EmployeesOnBusinessTripRevenueEstimatesService',
        function ($scope, EmployeesOnBusinessTripRevenueEstimatesService) {

            // Define
            $scope.IsSaveEmployeesOnBusinessTripRevenueEstimates = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeesOnBusinessTripIdTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorRevenue = false;
            }

            // ShowPopupEmployeesOnBusinessTripRevenueEstimates
            $scope.ShowPopupEmployeesOnBusinessTripRevenueEstimates = function (e, id) {
                $scope.IsSaveEmployeesOnBusinessTripRevenueEstimates = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveEmployeesOnBusinessTripRevenueEstimates").prop("disabled", false);
                $("#btnCloseEmployeesOnBusinessTripRevenueEstimates").prop("disabled", false);

                $scope.modelEmployeesOnBusinessTripRevenueEstimates = {
                    Id: 0,
                    EmployeesOnBusinessTripId: STRING_EMPTY,
                    Revenue: STRING_EMPTY,
                    AmountOfMoney: 0,
                    Note: STRING_EMPTY
                };

                var windowEmployeesOnBusinessTripRevenueEstimates = $("#KenWindownEmployeesOnBusinessTripRevenueEstimates").kendoWindow({
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
                    case "ADD_EMPLOYEESONBUSINESSTRIPREVENUEESTIMATES":
                        loadingPopUp();
                        windowEmployeesOnBusinessTripRevenueEstimates.title("Thêm mới");
                        windowEmployeesOnBusinessTripRevenueEstimates.open();
                        windowEmployeesOnBusinessTripRevenueEstimates.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_EMPLOYEESONBUSINESSTRIPREVENUEESTIMATES":
                        loadingPopUp();
                        EmployeesOnBusinessTripRevenueEstimatesService.GetAllEmployeesOnBusinessTripRevenueEstimates(id, $scope.EmployeesOnBusinessTripIdTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelEmployeesOnBusinessTripRevenueEstimates.Id = id;
                                $scope.modelEmployeesOnBusinessTripRevenueEstimates.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                                $scope.modelEmployeesOnBusinessTripRevenueEstimates.Revenue = response.data[0].Revenue;
                                $scope.modelEmployeesOnBusinessTripRevenueEstimates.AmountOfMoney = response.data[0].AmountOfMoney;
                                $scope.modelEmployeesOnBusinessTripRevenueEstimates.Note = response.data[0].Note;
                                windowEmployeesOnBusinessTripRevenueEstimates.title("Sửa dữ liệu");
                                windowEmployeesOnBusinessTripRevenueEstimates.open();
                                windowEmployeesOnBusinessTripRevenueEstimates.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_EMPLOYEESONBUSINESSTRIPREVENUEESTIMATES":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelEmployeesOnBusinessTripRevenueEstimates);
                                data.Id = id;
                                data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;
                                EmployeesOnBusinessTripRevenueEstimatesService.DeleteEmployeesOnBusinessTripRevenueEstimates(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("REVENUE_ESTIMATION");
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

            // CloseEmployeesOnBusinessTripRevenueEstimates
            $scope.CloseEmployeesOnBusinessTripRevenueEstimates = function () {
                $scope.IsSaveEmployeesOnBusinessTripRevenueEstimates = false;
                onShowMessageValidate();
                $("#KenWindownEmployeesOnBusinessTripRevenueEstimates").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueEmployeesOnBusinessTripRevenueEstimates
            $scope.onChangeValueEmployeesOnBusinessTripRevenueEstimates = function (e) {
                switch (e) {
                    case "Revenue":
                        if ($scope.modelEmployeesOnBusinessTripRevenueEstimates.Revenue !== STRING_EMPTY) {
                            $scope.showHasErrorRevenue = false;
                        } else {
                            $scope.showHasErrorRevenue = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveEmployeesOnBusinessTripRevenueEstimates
            $scope.SaveEmployeesOnBusinessTripRevenueEstimates = function (form) {
                $scope.IsSaveEmployeesOnBusinessTripRevenueEstimates = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelEmployeesOnBusinessTripRevenueEstimates.Revenue || $scope.modelEmployeesOnBusinessTripRevenueEstimates.Revenue === STRING_EMPTY) {
                        $scope.showHasErrorRevenue = true;
                    }
                    if ($scope.showHasErrorRevenue === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelEmployeesOnBusinessTripRevenueEstimates);
                data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripIdTemp;

                loadingPopUp();
                $("#btnSaveEmployeesOnBusinessTripRevenueEstimates").prop("disabled", true);
                $("#btnCloseEmployeesOnBusinessTripRevenueEstimates").prop("disabled", true);
                EmployeesOnBusinessTripRevenueEstimatesService.SaveEmployeesOnBusinessTripRevenueEstimates(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownEmployeesOnBusinessTripRevenueEstimates").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("REVENUE_ESTIMATION");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownEmployeesOnBusinessTripRevenueEstimates").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
