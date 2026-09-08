(function (angular) {
    "use strict";
    hrmApp.controller('DegreeInformationController', [
        '$scope', 'DegreeInformationService',
        function ($scope, DegreeInformationService) {

            // Define
            $scope.IsSaveDegree = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;
            $scope.CheckToYear = false;
            $scope.MessageCheckToYear = "Đến năm phải lớn hơn hoặc bằng với Từ năm";

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorTrainingPlacesIdDegree = false;
            }

            // ShowPopupDegree
            $scope.ShowPopupDegree = function (e, id) {
                $scope.IsSaveDegree = false;
                $scope.CheckValidate = true;
                $scope.CheckToYear = false;
                var valueClick = e;
                $("#btnSaveDegree").prop("disabled", false);
                $("#btnCloseDegree").prop("disabled", false);

                $scope.modelDegree = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    TrainingPlacesId: STRING_EMPTY,
                    FromYear: STRING_EMPTY,
                    ToYear: STRING_EMPTY,
                    FacultyId: STRING_EMPTY,
                    SpecializedId: STRING_EMPTY,
                    DegreeTrainingId: STRING_EMPTY,
                    FormsOfTrainingId: STRING_EMPTY,
                    ClassificationId: STRING_EMPTY,
                    Graduated: false,
                    DateReceived: STRING_EMPTY,
                    Note: STRING_EMPTY
                };

                var windowDegree = $("#KenWindownDegree").kendoWindow({
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
                    case "ADD_DEGREE":
                        loadingPopUp();
                        windowDegree.title("Thêm mới");
                        windowDegree.open();
                        windowDegree.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_DEGREE":
                        loadingPopUp();
                        DegreeInformationService.GetAllDegreeInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelDegree.Id = id;
                                $scope.modelDegree.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelDegree.TrainingPlacesId = response.data[0].TrainingPlacesId;
                                $scope.modelDegree.FromYear = response.data[0].FromYear;
                                $scope.modelDegree.ToYear = response.data[0].ToYear;
                                $scope.modelDegree.FacultyId = response.data[0].FacultyId;
                                $scope.modelDegree.SpecializedId = response.data[0].SpecializedId;
                                $scope.modelDegree.DegreeTrainingId = response.data[0].DegreeTrainingId;
                                $scope.modelDegree.FormsOfTrainingId = response.data[0].FormsOfTrainingId;
                                $scope.modelDegree.ClassificationId = response.data[0].ClassificationId;
                                $scope.modelDegree.Graduated = response.data[0].Graduated;
                                $scope.modelDegree.DateReceived = kendo.parseDate(response.data[0].DateReceived, DATE_FORMAT);
                                $scope.modelDegree.Note = response.data[0].Note;
                                windowDegree.title("Sửa dữ liệu");
                                windowDegree.open();
                                windowDegree.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_DEGREE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelDegree);
                                data.Id = id;
                                data.EmployeeId = $scope.EmployeeTemp;
                                DegreeInformationService.DeleteDegreeInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("DEGREE");
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

            // CloseDegree
            $scope.CloseDegree = function () {
                $scope.IsSaveDegree = false;
                onShowMessageValidate();
                $("#KenWindownDegree").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueDegree
            $scope.onChangeValueDegree = function (e) {
                switch (e) {
                    case "TrainingPlacesIdDegree":
                        if ($scope.modelDegree.TrainingPlacesId !== STRING_EMPTY) {
                            $scope.showHasErrorTrainingPlacesIdDegree = false;
                        } else {
                            $scope.showHasErrorTrainingPlacesIdDegree = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveDegree
            $scope.SaveDegree = function (form) {
                $scope.IsSaveDegree = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelDegree.TrainingPlacesId || $scope.modelDegree.TrainingPlacesId === STRING_EMPTY) {
                        $scope.showHasErrorTrainingPlacesIdDegree = true;
                    }
                    if ($scope.showHasErrorTrainingPlacesIdDegree === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelDegree);
                data.EmployeeId = $scope.EmployeeTemp;
                data.DateReceived = kendo.parseDate($scope.modelDegree.DateReceived, DATE_FORMAT);

                // Check validate FromYear < ToYear
                if (data.FromYear != null && data.ToYear != null && parseInt(data.FromYear) > parseInt(data.ToYear)) {
                    $scope.CheckToYear = true;
                    return;
                }
                else {
                    $scope.CheckToYear = false;
                }

                loadingPopUp();
                $("#btnSaveDegree").prop("disabled", true);
                $("#btnCloseDegree").prop("disabled", true);
                DegreeInformationService.SaveDegreeInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownDegree").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("DEGREE");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownDegree").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // onChangeGraduated
            $scope.onChangeGraduated = function () {
                $scope.modelDegree.DateReceived = STRING_EMPTY;
            };

        }]);

})(window.angular);
