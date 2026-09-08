(function (angular) {
    "use strict";
    hrmApp.controller('FamilyInformationController', [
        '$scope', 'FamilyInformationService',
        function ($scope, FamilyInformationService) {

            // Define
            $scope.IsSaveFamily = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.CheckDateOfBirthLessDeadDate = false;
            $scope.MessageCheckDateDead = "Ngày mất phải lớn hơn Ngày sinh";

            $scope.CheckTimeToCalculateDeductionLessTimeToEndTheDeduction = false;
            $scope.MessageCheckDateDeduction = "Thời điểm tính GT phải nhỏ hơn Thời điểm kết thúc GT";

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorRelationshipId = false;
                $scope.showHasErrorFirstAndLastName = false;
                $scope.showHasErrorSexIdFamily = false;
            }

            // ShowPopupFamily
            $scope.ShowPopupFamily = function (e, id) {
                $scope.IsSaveFamily = false;
                $scope.CheckValidate = true;
                $scope.CheckDateOfBirthLessDeadDate = false;
                $scope.CheckTimeToCalculateDeductionLessTimeToEndTheDeduction = false;
                var valueClick = e;
                $("#btnSaveFamily").prop("disabled", false);
                $("#btnCloseFamily").prop("disabled", false);

                $scope.modelFamily = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    RelationshipId: STRING_EMPTY,
                    FirstAndLastName: STRING_EMPTY,
                    DateOfBirth: STRING_EMPTY,
                    SexId: STRING_EMPTY,
                    NationalityId: STRING_EMPTY,
                    IdPassportNumber: STRING_EMPTY,
                    Address: STRING_EMPTY,
                    MobilePhone: STRING_EMPTY,
                    HomePhone: STRING_EMPTY,
                    Email: STRING_EMPTY,
                    Job: STRING_EMPTY,
                    PersonalTaxCode: STRING_EMPTY,
                    Workplace: STRING_EMPTY,
                    SameHouseholdRegistrationBook: true,
                    BeTheHeadOfTheHousehold: false,
                    IsADependent: false,
                    TimeToCalculateDeduction: STRING_EMPTY,
                    TimeToEndTheDeduction: STRING_EMPTY,
                    Note: STRING_EMPTY,
                    IsDead: false,
                    DeadDate: STRING_EMPTY,
                    AsAnEmergencyContact: false,
                    Number: STRING_EMPTY,
                    NumberBook: STRING_EMPTY
                };

                var windowFamily = $("#KenWindownFamily").kendoWindow({
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
                    case "ADD_FAMILY":
                        loadingPopUp();
                        windowFamily.title("Thêm mới");
                        windowFamily.open();
                        windowFamily.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_FAMILY":
                        loadingPopUp();
                        FamilyInformationService.GetAllFamilyInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelFamily.Id = id;
                                $scope.modelFamily.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelFamily.RelationshipId = response.data[0].RelationshipId;
                                $scope.modelFamily.FirstAndLastName = response.data[0].FirstAndLastName;
                                $scope.modelFamily.DateOfBirth = kendo.parseDate(response.data[0].DateOfBirth, DATE_FORMAT);
                                $scope.modelFamily.SexId = response.data[0].SexId;
                                $scope.modelFamily.NationalityId = response.data[0].NationalityId;
                                $scope.modelFamily.IdPassportNumber = response.data[0].IdPassportNumber;
                                $scope.modelFamily.Address = response.data[0].Address;
                                $scope.modelFamily.MobilePhone = response.data[0].MobilePhone;
                                $scope.modelFamily.HomePhone = response.data[0].HomePhone;
                                $scope.modelFamily.Email = response.data[0].Email;
                                $scope.modelFamily.Job = response.data[0].Job;
                                $scope.modelFamily.PersonalTaxCode = response.data[0].PersonalTaxCode;
                                $scope.modelFamily.Workplace = response.data[0].Workplace;
                                $scope.modelFamily.SameHouseholdRegistrationBook = response.data[0].SameHouseholdRegistrationBook;
                                $scope.modelFamily.BeTheHeadOfTheHousehold = response.data[0].BeTheHeadOfTheHousehold;
                                $scope.modelFamily.IsADependent = response.data[0].IsADependent;
                                $scope.modelFamily.TimeToCalculateDeduction = kendo.parseDate(response.data[0].TimeToCalculateDeduction, DATE_FORMAT);
                                $scope.modelFamily.TimeToEndTheDeduction = kendo.parseDate(response.data[0].TimeToEndTheDeduction, DATE_FORMAT);
                                $scope.modelFamily.Note = response.data[0].Note;
                                $scope.modelFamily.IsDead = response.data[0].IsDead;
                                $scope.modelFamily.DeadDate = kendo.parseDate(response.data[0].DeadDate, DATE_FORMAT);
                                $scope.modelFamily.AsAnEmergencyContact = response.data[0].AsAnEmergencyContact;
                                $scope.modelFamily.Number = response.data[0].Number;
                                $scope.modelFamily.NumberBook = response.data[0].NumberBook;
                                windowFamily.title("Sửa dữ liệu");
                                windowFamily.open();
                                windowFamily.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_FAMILY":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelFamily);
                                data.Id = id;
                                FamilyInformationService.DeleteFamilyInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("FAMILY");
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

            // CloseFamily
            $scope.CloseFamily = function () {
                $scope.IsSaveFamily = false;
                onShowMessageValidate();
                $("#KenWindownFamily").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueFamily
            $scope.onChangeValueFamily = function (e) {
                switch (e) {
                    case "RelationshipId":
                        if ($scope.modelFamily.RelationshipId !== STRING_EMPTY) {
                            $scope.showHasErrorRelationshipId = false;
                        } else {
                            $scope.showHasErrorRelationshipId = true;
                        }
                        break;
                    case "FirstAndLastName":
                        if ($scope.modelFamily.FirstAndLastName !== STRING_EMPTY) {
                            $scope.showHasErrorFirstAndLastName = false;
                        } else {
                            $scope.showHasErrorFirstAndLastName = true;
                        }
                        break;
                    case "SexIdFamily":
                        if ($scope.modelFamily.SexId !== STRING_EMPTY) {
                            $scope.showHasErrorSexIdFamily = false;
                        } else {
                            $scope.showHasErrorSexIdFamily = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveFamily
            $scope.SaveFamily = function (form) {
                $scope.IsSaveFamily = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelFamily.RelationshipId || $scope.modelFamily.RelationshipId === STRING_EMPTY) {
                        $scope.showHasErrorRelationshipId = true;
                    }
                    if (!$scope.modelFamily.FirstAndLastName || $scope.modelFamily.FirstAndLastName === STRING_EMPTY) {
                        $scope.showHasErrorFirstAndLastName = true;
                    }
                    if (!$scope.modelFamily.SexId || $scope.modelFamily.SexId === STRING_EMPTY) {
                        $scope.showHasErrorSexIdFamily = true;
                    }
                    if ($scope.showHasErrorRelationshipId === true || $scope.showHasErrorFirstAndLastName === true || $scope.showHasErrorSexIdFamily === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelFamily);
                data.EmployeeId = $scope.EmployeeTemp;
                data.DateOfBirth = kendo.parseDate($scope.modelFamily.DateOfBirth, DATE_FORMAT);
                data.TimeToCalculateDeduction = kendo.parseDate($scope.modelFamily.TimeToCalculateDeduction, DATE_FORMAT);
                data.TimeToEndTheDeduction = kendo.parseDate($scope.modelFamily.TimeToEndTheDeduction, DATE_FORMAT);
                data.DeadDate = kendo.parseDate($scope.modelFamily.DeadDate, DATE_FORMAT);

                // Check validate DateOfBirth < DeadDate
                if (data.DateOfBirth != null && data.DeadDate != null && data.DateOfBirth.getTime() >= data.DeadDate.getTime()) {
                    $scope.CheckDateOfBirthLessDeadDate = true;
                    return;
                }
                else {
                    $scope.CheckDateOfBirthLessDeadDate = false;
                }

                // Check validate TimeToCalculateDeduction < TimeToEndTheDeduction
                if (data.TimeToCalculateDeduction != null && data.TimeToEndTheDeduction != null && data.TimeToCalculateDeduction.getTime() > data.TimeToEndTheDeduction.getTime()) {
                    $scope.CheckTimeToCalculateDeductionLessTimeToEndTheDeduction = true;
                    return;
                } else {
                    $scope.CheckTimeToCalculateDeductionLessTimeToEndTheDeduction = false;
                }

                loadingPopUp();
                $("#btnSaveFamily").prop("disabled", true);
                $("#btnCloseFamily").prop("disabled", true);
                FamilyInformationService.SaveFamilyInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownFamily").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("FAMILY");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownFamily").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // onChangeIsDead
            $scope.onChangeIsDead = function () {
                $scope.modelFamily.DeadDate = STRING_EMPTY;
                $scope.modelFamily.AsAnEmergencyContact = false;
                $scope.CheckDateOfBirthLessDeadDate = false;
            };

            // onChangeIsADependent
            $scope.onChangeIsADependent = function () {
                $scope.modelFamily.TimeToCalculateDeduction = STRING_EMPTY;
                $scope.modelFamily.TimeToEndTheDeduction = STRING_EMPTY;
                $scope.CheckTimeToCalculateDeductionLessTimeToEndTheDeduction = false;
            };

        }]);

})(window.angular);
