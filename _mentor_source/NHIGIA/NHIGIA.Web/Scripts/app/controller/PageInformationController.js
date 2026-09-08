(function (angular) {
    "use strict";
    hrmApp.controller('PageInformationController', [
        '$scope', 'PageInformationService',
        function ($scope, PageInformationService) {

            // Define
            $scope.IsSavePage = false;
            $scope.CheckValidate = false;
            $scope.CheckExpirationDate = false;
            $scope.MessageCheckExpirationDate = "Ngày hết hạn phải lớn hơn hoặc bằng Ngày cấp";

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorNameOfPapers = false;
            }

            // ShowPopupPage
            $scope.ShowPopupPage = function (e, id) {
                $scope.IsSavePage = false;
                $scope.CheckValidate = true;
                $scope.CheckExpirationDate = false;
                var valueClick = e;
                $("#btnSavePage").prop("disabled", false);
                $("#btnClosePage").prop("disabled", false);

                $scope.modelPage = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    NameOfPapers: STRING_EMPTY,
                    IssuedBy: STRING_EMPTY,
                    DateRange: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ExpirationDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    Note: STRING_EMPTY
                };

                var windowPage = $("#KenWindownPage").kendoWindow({
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
                    case "ADD_PAGE":
                        loadingPopUp();
                        windowPage.title("Thêm mới");
                        windowPage.open();
                        windowPage.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_PAGE":
                        loadingPopUp();
                        PageInformationService.GetAllPageInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelPage.Id = id;
                                $scope.modelPage.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelPage.NameOfPapers = response.data[0].NameOfPapers;
                                $scope.modelPage.IssuedBy = response.data[0].IssuedBy;
                                $scope.modelPage.DateRange = kendo.parseDate(response.data[0].DateRange, DATE_FORMAT);
                                $scope.modelPage.ExpirationDate = kendo.parseDate(response.data[0].ExpirationDate, DATE_FORMAT);
                                $scope.modelPage.Note = response.data[0].Note;
                                windowPage.title("Sửa dữ liệu");
                                windowPage.open();
                                windowPage.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_PAGE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelPage);
                                data.Id = id;
                                PageInformationService.DeletePageInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("PAGE");
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

            // ClosePage
            $scope.ClosePage = function () {
                $scope.IsSavePage = false;
                onShowMessageValidate();
                $("#KenWindownPage").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValuePage
            $scope.onChangeValuePage = function (e) {
                switch (e) {
                    case "NameOfPapers":
                        if ($scope.modelPage.NameOfPapers !== STRING_EMPTY) {
                            $scope.showHasErrorNameOfPapers = false;
                        } else {
                            $scope.showHasErrorNameOfPapers = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SavePage
            $scope.SavePage = function (form) {
                $scope.IsSavePage = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelPage.NameOfPapers || $scope.modelPage.NameOfPapers === STRING_EMPTY) {
                        $scope.showHasErrorNameOfPapers = true;
                    }
                    if ($scope.showHasErrorNameOfPapers === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelPage);
                data.EmployeeId = $scope.EmployeeTemp;
                data.DateRange = kendo.parseDate($scope.modelPage.DateRange, DATE_FORMAT);
                data.ExpirationDate = kendo.parseDate($scope.modelPage.ExpirationDate, DATE_FORMAT);

                // Check DateRange < ExpirationDate
                if (data.DateRange !== null && data.ExpirationDate !== null && data.DateRange.getTime() > data.ExpirationDate.getTime()) {
                    $scope.CheckExpirationDate = true;
                    return;
                } else {
                    $scope.CheckExpirationDate = false;
                }

                loadingPopUp();
                $("#btnSavePage").prop("disabled", true);
                $("#btnClosePage").prop("disabled", true);
                PageInformationService.SavePageInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownPage").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("PAGE");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownPage").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };
        }]);

})(window.angular);
