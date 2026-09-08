(function (angular) {
    "use strict";
    hrmApp.controller('ContractInformationController', [
        '$scope', 'ContractInformationService',
        function ($scope, ContractInformationService) {

            // Define
            $scope.IsSaveContract = false;
            $scope.CheckValidate = false;

            $scope.CheckExpirationDate = false;
            $scope.MessageCheckExpirationDate = "Ngày hết hạn phải lớn hơn hoặc bằng ngày có hiệu lực hợp đồng";

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorContractName = false;
                $scope.showHasErrorContractTypeIdContract = false;
            }

            // ShowPopupContract
            $scope.ShowPopupContract = function (e, id) {
                $scope.IsSaveContract = false;
                $scope.CheckExpirationDate = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveContract").prop("disabled", false);
                $("#btnCloseContract").prop("disabled", false);
                $(".custom_hidden_contract").removeClass("hidden");
                $(".custom_hidden_contract").addClass("show");

                $scope.modelContract = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    JobPositionId: STRING_EMPTY,
                    SomeContracts: STRING_EMPTY,
                    WorkUnitId: STRING_EMPTY,
                    SignDay: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ContractName: STRING_EMPTY,
                    ContractTypeId: STRING_EMPTY,
                    ContractTermId: STRING_EMPTY,
                    TheFormOfWorkId: STRING_EMPTY,
                    WageRate: 100,
                    EffectiveDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ExpirationDate: STRING_EMPTY,
                    Abstract: STRING_EMPTY,
                    Note: STRING_EMPTY,
                    EmployeeName: STRING_EMPTY,
                    EmployeeCode: STRING_EMPTY
                };

                var windowContract = $("#KenWindownContract").kendoWindow({
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
                    case "ADD_CONTRACT":
                        loadingPopUp();
                        $scope.modelContract.EmployeeName = $scope.$parent.model.EmployeeName;
                        $scope.modelContract.EmployeeCode = $scope.$parent.model.EmployeeCode;
                        $scope.modelContract.ContractTypeId = $scope.$parent.model.ContractTypeId;
                        $scope.modelContract.WorkUnitId = $scope.$parent.model.WorkUnitId;
                        $scope.modelContract.JobPositionId = $scope.$parent.model.JobPositionId;
                        windowContract.title("Thêm mới");
                        windowContract.open();
                        windowContract.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_CONTRACT":
                        loadingPopUp();
                        ContractInformationService.GetAllContractInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelContract.Id = id;
                                $scope.modelContract.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelContract.JobPositionId = response.data[0].JobPositionId;
                                $scope.modelContract.SomeContracts = response.data[0].SomeContracts;
                                $scope.modelContract.WorkUnitId = response.data[0].WorkUnitId;
                                $scope.modelContract.SignDay = kendo.parseDate(response.data[0].SignDay, DATE_FORMAT);
                                $scope.modelContract.ContractName = response.data[0].ContractName;
                                $scope.modelContract.ContractTypeId = response.data[0].ContractTypeId;
                                $scope.modelContract.ContractTermId = response.data[0].ContractTermId;
                                $scope.modelContract.TheFormOfWorkId = response.data[0].TheFormOfWorkId;
                                $scope.modelContract.WageRate = response.data[0].WageRate;
                                $scope.modelContract.EffectiveDate = kendo.parseDate(response.data[0].EffectiveDate, DATE_FORMAT);
                                $scope.modelContract.ExpirationDate = kendo.parseDate(response.data[0].ExpirationDate, DATE_FORMAT);
                                $scope.modelContract.Abstract = response.data[0].Abstract;
                                $scope.modelContract.Note = response.data[0].Note;
                                $scope.modelContract.EmployeeName = $scope.$parent.model.EmployeeName;
                                $scope.modelContract.EmployeeCode = $scope.$parent.model.EmployeeCode;
                                windowContract.title("Sửa dữ liệu");
                                windowContract.open();
                                windowContract.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "VIEW_CONTRACT":
                        loadingPopUp();
                        $(".custom_hidden_contract").removeClass("show");
                        $(".custom_hidden_contract").addClass("hidden");
                        ContractInformationService.GetAllContractInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelContract.Id = id;
                                $scope.modelContract.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelContract.JobPositionId = response.data[0].JobPositionId;
                                $scope.modelContract.SomeContracts = response.data[0].SomeContracts;
                                $scope.modelContract.WorkUnitId = response.data[0].WorkUnitId;
                                $scope.modelContract.SignDay = kendo.parseDate(response.data[0].SignDay, DATE_FORMAT);
                                $scope.modelContract.ContractName = response.data[0].ContractName;
                                $scope.modelContract.ContractTypeId = response.data[0].ContractTypeId;
                                $scope.modelContract.ContractTermId = response.data[0].ContractTermId;
                                $scope.modelContract.TheFormOfWorkId = response.data[0].TheFormOfWorkId;
                                $scope.modelContract.WageRate = response.data[0].WageRate;
                                $scope.modelContract.EffectiveDate = kendo.parseDate(response.data[0].EffectiveDate, DATE_FORMAT);
                                $scope.modelContract.ExpirationDate = kendo.parseDate(response.data[0].ExpirationDate, DATE_FORMAT);
                                $scope.modelContract.Abstract = response.data[0].Abstract;
                                $scope.modelContract.Note = response.data[0].Note;
                                $scope.modelContract.EmployeeName = $scope.$parent.model.EmployeeName;
                                $scope.modelContract.EmployeeCode = $scope.$parent.model.EmployeeCode;
                                windowContract.title("Xem hợp đồng");
                                windowContract.open();
                                windowContract.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            // CloseContract
            $scope.CloseContract = function () {
                $scope.IsSaveContract = false;
                onShowMessageValidate();
                $("#KenWindownContract").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueContract
            $scope.onChangeValueContract = function (e) {
                switch (e) {
                    case "ContractName":
                        if ($scope.modelContract.ContractName !== STRING_EMPTY) {
                            $scope.showHasErrorContractName = false;
                        } else {
                            $scope.showHasErrorContractName = true;
                        }
                        break;
                    case "ContractTypeIdContract":
                        if ($scope.modelContract.ContractTypeId !== STRING_EMPTY) {
                            $scope.showHasErrorContractTypeIdContract = false;
                        } else {
                            $scope.showHasErrorContractTypeIdContract = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveContract
            $scope.SaveContract = function (form) {
                $scope.IsSaveContract = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelContract.ContractName || $scope.modelContract.ContractName === STRING_EMPTY) {
                        $scope.showHasErrorContractName = true;
                    }
                    if (!$scope.modelContract.ContractTypeId || $scope.modelContract.ContractTypeId === STRING_EMPTY) {
                        $scope.showHasErrorContractTypeIdContract = true;
                    }
                    if ($scope.showHasErrorContractName === true || $scope.showHasErrorContractTypeIdContract === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelContract);
                data.EmployeeId = $scope.EmployeeTemp;
                data.SignDay = kendo.parseDate($scope.modelContract.SignDay, DATE_FORMAT);
                data.EffectiveDate = kendo.parseDate($scope.modelContract.EffectiveDate, DATE_FORMAT);
                data.ExpirationDate = kendo.parseDate($scope.modelContract.ExpirationDate, DATE_FORMAT);

                // Check validate ExpirationDate > EffectiveDate
                if (data.EffectiveDate != null && data.ExpirationDate != null && data.ExpirationDate.getTime() < data.EffectiveDate.getTime()) {
                    $scope.CheckExpirationDate = true;
                    return;
                }
                else {
                    $scope.CheckExpirationDate = false;
                }

                loadingPopUp();
                $("#btnSaveContract").prop("disabled", true);
                $("#btnCloseContract").prop("disabled", true);
                if ($scope.modelContract.Id > 0) {
                    ContractInformationService.SaveContractInformation(data).then(function success(response) {
                        if (response.data.status === 200) {
                            $("#KenWindownContract").data("kendoWindow").close();
                            bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                            $scope.$parent.ClickInformation("CONTRACT");
                            stopLoadingPopUp();
                        } else {
                            $("#KenWindownContract").data("kendoWindow").close();
                            bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                            stopLoadingPopUp();
                        }
                    });
                } else {
                    if ($scope.GetAllContractInformation.length > 0) {
                        var r = confirm(MSG_INSERT_CONTRACT_CONFIRM);
                        if (r == true) {
                            ContractInformationService.SaveContractInformation(data).then(function success(response) {
                                if (response.data.status === 200) {
                                    $("#KenWindownContract").data("kendoWindow").close();
                                    bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                    $scope.$parent.ClickInformation("CONTRACT");
                                    stopLoadingPopUp();
                                } else {
                                    $("#KenWindownContract").data("kendoWindow").close();
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    stopLoadingPopUp();
                                }
                            });
                        } else {
                            $("#btnSaveContract").prop("disabled", false);
                            $("#btnCloseContract").prop("disabled", false);
                            stopLoadingPopUp();
                        }
                    } else {
                        ContractInformationService.SaveContractInformation(data).then(function success(response) {
                            if (response.data.status === 200) {
                                $("#KenWindownContract").data("kendoWindow").close();
                                bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                $scope.$parent.ClickInformation("CONTRACT");
                                stopLoadingPopUp();
                            } else {
                                $("#KenWindownContract").data("kendoWindow").close();
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                    }
                }

            };

        }]);

})(window.angular);
