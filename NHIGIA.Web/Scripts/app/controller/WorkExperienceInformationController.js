(function (angular) {
    "use strict";
    hrmApp.controller('WorkExperienceInformationController', [
        '$scope', 'WorkExperienceInformationService',
        function ($scope, WorkExperienceInformationService) {

            // Define
            $scope.IsSaveWorkExperience = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.CheckByMonthAndYear = false;
            $scope.MessageCheckByMonthAndYear = "Đến tháng, năm phải lớn hơn hoặc bằng Từ tháng, năm";

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorWorkplace = false;
                $scope.showHasErrorJobPosition = false;
            }

            // ShowPopupWorkExperience
            $scope.ShowPopupWorkExperience = function (e, id) {
                $scope.IsSaveWorkExperience = false;
                $scope.CheckValidate = true;
                $scope.CheckByMonthAndYear = false;
                var valueClick = e;
                $("#btnSaveWorkExperience").prop("disabled", false);
                $("#btnCloseWorkExperience").prop("disabled", false);

                $scope.modelWorkExperience = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    FromMonthAndYear: (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ByMonthAndYear: (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    Workplace: STRING_EMPTY,
                    JobPosition: STRING_EMPTY,
                    Wage: STRING_EMPTY,
                    JobDescription: STRING_EMPTY,
                    Note: STRING_EMPTY,
                    FirstAndLastName: STRING_EMPTY,
                    Title: STRING_EMPTY,
                    Phone: STRING_EMPTY,
                    Email: STRING_EMPTY,
                    HaveCheckedCompared: false
                };

                var windowWorkExperience = $("#KenWindownWorkExperience").kendoWindow({
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
                    case "ADD_WORKEXPERIENCE":
                        loadingPopUp();
                        windowWorkExperience.title("Thêm mới");
                        windowWorkExperience.open();
                        windowWorkExperience.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_WORKEXPERIENCE":
                        loadingPopUp();
                        WorkExperienceInformationService.GetAllWorkExperienceInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelWorkExperience.Id = id;
                                $scope.modelWorkExperience.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelWorkExperience.FromMonthAndYear = kendo.parseDate(response.data[0].FromMonthAndYear, DATE_FORMAT_MONTH);
                                $scope.modelWorkExperience.ByMonthAndYear = kendo.parseDate(response.data[0].ByMonthAndYear, DATE_FORMAT_MONTH);
                                $scope.modelWorkExperience.Workplace = response.data[0].Workplace;
                                $scope.modelWorkExperience.JobPosition = response.data[0].JobPosition;
                                $scope.modelWorkExperience.Wage = response.data[0].Wage;
                                $scope.modelWorkExperience.JobDescription = response.data[0].JobDescription;
                                $scope.modelWorkExperience.Note = response.data[0].Note;
                                $scope.modelWorkExperience.FirstAndLastName = response.data[0].FirstAndLastName;
                                $scope.modelWorkExperience.Title = response.data[0].Title;
                                $scope.modelWorkExperience.Phone = response.data[0].Phone;
                                $scope.modelWorkExperience.Email = response.data[0].Email;
                                $scope.modelWorkExperience.HaveCheckedCompared = response.data[0].HaveCheckedCompared;
                                windowWorkExperience.title("Sửa dữ liệu");
                                windowWorkExperience.open();
                                windowWorkExperience.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_WORKEXPERIENCE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelWorkExperience);
                                data.Id = id;
                                WorkExperienceInformationService.DeleteWorkExperienceInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("WORKEXPERIENCE");
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

            // CloseWorkExperience
            $scope.CloseWorkExperience = function () {
                $scope.IsSaveWorkExperience = false;
                onShowMessageValidate();
                $("#KenWindownWorkExperience").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueWorkExperience
            $scope.onChangeValueWorkExperience = function (e) {
                switch (e) {
                    case "Workplace":
                        if ($scope.modelWorkExperience.Workplace !== STRING_EMPTY) {
                            $scope.showHasErrorWorkplace = false;
                        } else {
                            $scope.showHasErrorWorkplace = true;
                        }
                        break;
                    case "JobPosition":
                        if ($scope.modelWorkExperience.JobPosition !== STRING_EMPTY) {
                            $scope.showHasErrorJobPosition = false;
                        } else {
                            $scope.showHasErrorJobPosition = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveWorkExperience
            $scope.SaveWorkExperience = function (form) {
                $scope.IsSaveWorkExperience = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelWorkExperience.Workplace || $scope.modelWorkExperience.Workplace === STRING_EMPTY) {
                        $scope.showHasErrorWorkplace = true;
                    }
                    if (!$scope.modelWorkExperience.JobPosition || $scope.modelWorkExperience.JobPosition === STRING_EMPTY) {
                        $scope.showHasErrorJobPosition = true;
                    }

                    if ($scope.showHasErrorWorkplace === true || $scope.showHasErrorJobPosition === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelWorkExperience);
                data.EmployeeId = $scope.EmployeeTemp;
                data.FromMonthAndYear = kendo.parseDate($scope.modelWorkExperience.FromMonthAndYear, DATE_FORMAT_MONTH);
                data.ByMonthAndYear = kendo.parseDate($scope.modelWorkExperience.ByMonthAndYear, DATE_FORMAT_MONTH);

                // Check validate FromMonthAndYear < ByMonthAndYear
                if (data.FromMonthAndYear != null && data.ByMonthAndYear != null && data.FromMonthAndYear.getTime() > data.ByMonthAndYear.getTime()) {
                    $scope.CheckByMonthAndYear = true;
                    return;
                }
                else {
                    $scope.CheckByMonthAndYear = false;
                }

                loadingPopUp();
                $("#btnSaveWorkExperience").prop("disabled", true);
                $("#btnCloseWorkExperience").prop("disabled", true);
                WorkExperienceInformationService.SaveWorkExperienceInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownWorkExperience").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("WORKEXPERIENCE");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownWorkExperience").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            $scope.monthSelectorOptions = {
                start: "year",
                depth: "year"
            };
        }]);

})(window.angular);
