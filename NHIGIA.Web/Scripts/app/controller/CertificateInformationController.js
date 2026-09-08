(function (angular) {
    "use strict";
    hrmApp.controller('CertificateInformationController', [
        '$scope', 'CertificateInformationService',
        function ($scope, CertificateInformationService) {

            // Define
            $scope.IsSaveCertificate = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.CheckExpirationDate = false;
            $scope.MessageCheckExpirationDate = "Ngày hết hạn phải lớn hơn hoặc bằng Ngày cấp";

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorCertificateGroupId = false;
                $scope.showHasErrorCertificateName = false;
            }

            // ShowPopupCertificate
            $scope.ShowPopupCertificate = function (e, id) {
                $scope.IsSaveCertificate = false;
                $scope.CheckValidate = true;
                $scope.CheckExpirationDate = false;

                var valueClick = e;
                $("#btnSaveCertificate").prop("disabled", false);
                $("#btnCloseCertificate").prop("disabled", false);

                $scope.modelCertificate = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    CertificateGroupId: STRING_EMPTY,
                    CertificateName: STRING_EMPTY,
                    NumberOfCertificates: STRING_EMPTY,
                    DegreeTrainingId: STRING_EMPTY,
                    DateRange: STRING_EMPTY,
                    ExpirationDate: STRING_EMPTY,
                    IssuedBy: STRING_EMPTY,
                    ClassificationId: STRING_EMPTY,
                    Note: STRING_EMPTY
                };

                var windowCertificate = $("#KenWindownCertificate").kendoWindow({
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
                    case "ADD_CERTIFICATE":
                        loadingPopUp();
                        windowCertificate.title("Thêm mới");
                        windowCertificate.open();
                        windowCertificate.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_CERTIFICATE":
                        loadingPopUp();
                        CertificateInformationService.GetAllCertificateInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelCertificate.Id = id;
                                $scope.modelCertificate.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelCertificate.CertificateGroupId = response.data[0].CertificateGroupId;
                                $scope.modelCertificate.CertificateName = response.data[0].CertificateName;
                                $scope.modelCertificate.NumberOfCertificates = response.data[0].NumberOfCertificates;
                                $scope.modelCertificate.DegreeTrainingId = response.data[0].DegreeTrainingId;
                                $scope.modelCertificate.DateRange = kendo.parseDate(response.data[0].DateRange, DATE_FORMAT);
                                $scope.modelCertificate.ExpirationDate = kendo.parseDate(response.data[0].ExpirationDate, DATE_FORMAT);
                                $scope.modelCertificate.IssuedBy = response.data[0].IssuedBy;
                                $scope.modelCertificate.ClassificationId = response.data[0].ClassificationId;
                                $scope.modelCertificate.Note = response.data[0].Note;
                                windowCertificate.title("Sửa dữ liệu");
                                windowCertificate.open();
                                windowCertificate.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_CERTIFICATE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelCertificate);
                                data.Id = id;
                                CertificateInformationService.DeleteCertificateInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("CERTIFICATE");
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

            // CloseCertificate
            $scope.CloseCertificate = function () {
                $scope.IsSaveCertificate = false;
                onShowMessageValidate();
                $("#KenWindownCertificate").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueCertificate
            $scope.onChangeValueCertificate = function (e) {
                switch (e) {
                    case "CertificateGroupId":
                        if ($scope.modelCertificate.CertificateGroupId !== STRING_EMPTY) {
                            $scope.showHasErrorCertificateGroupId = false;
                        } else {
                            $scope.showHasErrorCertificateGroupId = true;
                        }
                        break;
                    case "CertificateName":
                        if ($scope.modelCertificate.CertificateName !== STRING_EMPTY) {
                            $scope.showHasErrorCertificateName = false;
                        } else {
                            $scope.showHasErrorCertificateName = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveCertificate
            $scope.SaveCertificate = function (form) {
                $scope.IsSaveCertificate = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelCertificate.CertificateGroupId || $scope.modelCertificate.CertificateGroupId === STRING_EMPTY) {
                        $scope.showHasErrorCertificateGroupId = true;
                    }
                    if (!$scope.modelCertificate.CertificateName || $scope.modelCertificate.CertificateName === STRING_EMPTY) {
                        $scope.showHasErrorCertificateName = true;
                    }

                    if ($scope.showHasErrorCertificateGroupId === true || $scope.showHasErrorCertificateName === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelCertificate);
                data.EmployeeId = $scope.EmployeeTemp;
                data.DateRange = kendo.parseDate($scope.modelCertificate.DateRange, DATE_FORMAT);
                data.ExpirationDate = kendo.parseDate($scope.modelCertificate.ExpirationDate, DATE_FORMAT);

                // Check validate DateRange < ExpirationDate
                if (data.DateRange != null && data.ExpirationDate != null && data.DateRange.getTime() > data.ExpirationDate.getTime()) {
                    $scope.CheckExpirationDate = true;
                    return;
                } else {
                    $scope.CheckExpirationDate = false;
                }

                loadingPopUp();
                $("#btnSaveCertificate").prop("disabled", true);
                $("#btnCloseCertificate").prop("disabled", true);
                CertificateInformationService.SaveCertificateInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownCertificate").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("CERTIFICATE");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownCertificate").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
