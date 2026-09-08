(function (angular) {
    "use strict";
    hrmApp.controller('AssetInformationController', [
        '$scope', 'AssetInformationService',
        function ($scope, AssetInformationService) {

            // Define
            $scope.IsSaveAsset = false;
            $scope.CheckValidate = false;
            $scope.CheckDuplicate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.CheckPayDay = false;
            $scope.MessageCheckPayDay = "Ngày nhận phải lớn hơn hoặc bằng ngày trả";

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorAssetCode = false;
                $scope.showHasErrorAssetName = false;
            }

            // ShowPopupAsset
            $scope.ShowPopupAsset = function (e, id) {
                $scope.IsSaveAsset = false;
                $scope.CheckValidate = true;
                $scope.CheckDuplicate = false;
                $scope.CheckPayDay = false;

                var valueClick = e;
                $("#btnSaveAsset").prop("disabled", false);
                $("#btnCloseAsset").prop("disabled", false);

                $scope.modelAsset = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    AssetCode: STRING_EMPTY,
                    AssetName: STRING_EMPTY,
                    AssetTypeId: STRING_EMPTY,
                    ReceivedDate: STRING_EMPTY,
                    PayDay: STRING_EMPTY,
                    AssetStatusId: STRING_EMPTY,
                    Note: STRING_EMPTY
                };

                var windowAsset = $("#KenWindownAsset").kendoWindow({
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
                    case "ADD_ASSET":
                        loadingPopUp();
                        windowAsset.title("Thêm mới");
                        windowAsset.open();
                        windowAsset.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_ASSET":
                        loadingPopUp();
                        AssetInformationService.GetAllAssetInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelAsset.Id = id;
                                $scope.modelAsset.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelAsset.AssetCode = response.data[0].AssetCode;
                                $scope.modelAsset.AssetName = response.data[0].AssetName;
                                $scope.modelAsset.AssetTypeId = response.data[0].AssetTypeId;
                                $scope.modelAsset.ReceivedDate = kendo.parseDate(response.data[0].ReceivedDate, DATE_FORMAT);
                                $scope.modelAsset.PayDay = kendo.parseDate(response.data[0].PayDay, DATE_FORMAT);
                                $scope.modelAsset.AssetStatusId = response.data[0].AssetStatusId;
                                $scope.modelAsset.Note = response.data[0].Note;
                                windowAsset.title("Sửa dữ liệu");
                                windowAsset.open();
                                windowAsset.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_ASSET":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelAsset);
                                data.Id = id;
                                AssetInformationService.DeleteAssetInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("ASSET");
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

            // CloseAsset
            $scope.CloseAsset = function () {
                $scope.IsSaveAsset = false;
                onShowMessageValidate();
                $("#KenWindownAsset").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueAsset
            $scope.onChangeValueAsset = function (e) {
                switch (e) {
                    case "AssetCode":
                        if ($scope.modelAsset.AssetCode !== STRING_EMPTY) {
                            $scope.showHasErrorAssetCode = false;
                        } else {
                            $scope.showHasErrorAssetCode = true;
                        }
                        break;
                    case "AssetName":
                        if ($scope.modelAsset.AssetName !== STRING_EMPTY) {
                            $scope.showHasErrorAssetName = false;
                        } else {
                            $scope.showHasErrorAssetName = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveAsset
            $scope.SaveAsset = function (form) {
                $scope.IsSaveAsset = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelAsset.AssetCode || $scope.modelAsset.AssetCode === STRING_EMPTY) {
                        $scope.showHasErrorAssetCode = true;
                    }
                    if (!$scope.modelAsset.AssetName || $scope.modelAsset.AssetName === STRING_EMPTY) {
                        $scope.showHasErrorAssetName = true;
                    }
                    if ($scope.showHasErrorAssetCode === true || $scope.showHasErrorAssetName === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelAsset);
                data.EmployeeId = $scope.EmployeeTemp;
                data.ReceivedDate = kendo.parseDate($scope.modelAsset.ReceivedDate, DATE_FORMAT);
                data.PayDay = kendo.parseDate($scope.modelAsset.PayDay, DATE_FORMAT);

                // Check validate ReceivedDate > PayDay
                if (data.ReceivedDate != null && data.PayDay != null && data.ReceivedDate.getTime() > data.PayDay.getTime()) {
                    $scope.CheckPayDay = true;
                    return;
                } else {
                    $scope.CheckPayDay = false;
                }

                loadingPopUp();
                $("#btnSaveAsset").prop("disabled", true);
                $("#btnCloseAsset").prop("disabled", true);
                AssetInformationService.SaveAssetInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownAsset").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("ASSET");
                        stopLoadingPopUp();
                    } else if (response.data.status === 409) {
                        $scope.CheckDuplicate = true;
                        $("#btnSaveAsset").prop("disabled", false);
                        $("#btnCloseAsset").prop("disabled", false);
                        $scope.MessageErrorForDuplicate = $scope.modelAsset.AssetCode + response.data.message;
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownAsset").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };
        }]);

})(window.angular);
