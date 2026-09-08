(function (angular) {
    "use strict";
    hrmApp.controller('AttachmentInformationController', [
        '$scope', 'AttachmentInformationService',
        function ($scope, AttachmentInformationService) {

            // Define
            $scope.IsSaveAttachment = false;
            $scope.CheckValidate = false;
            $scope.showHasErrorFileName = false;
            $scope.Attachment = [];

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;
            $scope.EmployeesOnBusinessTripTemp = $scope.$parent.EmployeesOnBusinessTripIdTemp;

            // ShowPopupAttachment
            $scope.ShowPopupAttachment = function (e, id) {
                $scope.IsSaveAttachment = false;
                $scope.CheckValidate = true;
                $scope.showHasErrorFileName = false;
                $scope.Attachment = [];
                var valueClick = e;
                $("#btnSaveAttachment").prop("disabled", false);
                $("#btnCloseAttachment").prop("disabled", false);

                $scope.modelAttachment = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    EmployeesOnBusinessTripId: STRING_EMPTY,
                    FileName: STRING_EMPTY,
                    FileType: STRING_EMPTY,
                    FileSize: STRING_EMPTY,
                    FileContent: STRING_EMPTY,
                    IsDownload: false
                };

                var windowAttachment = $("#KenWindownAttachment").kendoWindow({
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
                    case "ADD_ATTACHMENT":
                        loadingPopUp();
                        windowAttachment.title("Thêm mới");
                        windowAttachment.open();
                        windowAttachment.center();
                        stopLoadingPopUp();
                        break;
                    case "CHANGE_ATTACHMENT":
                        loadingPopUp();
                        var chkBox = document.getElementById("IsDownload_" + id);
                        var data = angular.copy($scope.modelAttachment);
                        data.Id = id;
                        data.IsDownload = chkBox.checked;
                        AttachmentInformationService.SaveAttachmentInformation(data).then(function success(response) {
                            if (response.data.status === 200) {
                                $scope.$parent.ClickInformation("ATTACHMENT");
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_ATTACHMENT":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelAttachment);
                                data.Id = id;
                                AttachmentInformationService.DeleteAttachmentInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("ATTACHMENT");
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

            // CloseAttachment
            $scope.CloseAttachment = function () {
                $scope.IsSaveAttachment = false;
                $("#KenWindownAttachment").closest(".k-window-content").data("kendoWindow").close();
            };

            // SaveAttachment
            $scope.SaveAttachment = function (form) {
                $scope.IsSaveAttachment = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelAttachment.FileName || $scope.modelAttachment.FileName === STRING_EMPTY) {
                        $scope.showHasErrorFileName = true;
                        return;
                    } else {
                        $scope.showHasErrorFileName = false;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelAttachment);
                if ($scope.EmployeesOnBusinessTripTemp && $scope.EmployeesOnBusinessTripTemp !== 0) {
                    data.EmployeeId = 0;
                    data.EmployeesOnBusinessTripId = $scope.EmployeesOnBusinessTripTemp;
                } else {
                    data.EmployeeId = $scope.EmployeeTemp;
                    data.EmployeesOnBusinessTripId = 0;
                }

                loadingPopUp();
                $("#btnSaveAttachment").prop("disabled", true);
                $("#btnCloseAttachment").prop("disabled", true);
                AttachmentInformationService.SaveAttachmentInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $(".k-upload-selected").click();
                        $("#KenWindownAttachment").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("ATTACHMENT");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownAttachment").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // upload error
            $scope.OnErrorAction = function (e) {
                $scope.errMessage = e.response.result;
            };

            // upload success
            $scope.OnSuccessAction = function (e) {
                $scope.Attachment = $scope.FileName;
                if (e.response.status === 1) {
                    //abc
                } else {
                    $scope.errMessage = e.response.result;
                }
            };

            // select file upload function
            $scope.onSelect = function (e) {
                if ($scope.Attachment === null || $scope.Attachment === undefined) {
                    $scope.Attachment = [];
                }

                if (e === undefined || e.files === null || e.files === undefined) {
                    return;
                }

                e.files.forEach(function (file) {
                    var item = {
                        FileName: file.name,
                        Size: file.size,
                        Extension: file.extension,
                        Type: file.rawFile.type

                    };
                    $scope.Attachment.forEach(function (file) {
                        if (file.name === item.FileName) {
                            $scope.Attachment.splice($scope.Attachment.indexOf(file), 1);
                        }
                    });

                    // upload 1 file ang replace file cu
                    $scope.Attachment = [item];

                    // upload 1 list
                    //$scope.Attachment.push(item);
                });

                $scope.modelAttachment.FileName = $scope.Attachment[0].FileName;

                if ($scope.modelAttachment.FileName || $scope.modelAttachment.FileName !== STRING_EMPTY) {
                    $scope.showHasErrorFileName = false;
                }

                $scope.$applyAsync();
            };

            //Delete FileAttachment
            $scope.DeleteFileUpload = function (item) {
                $scope.Attachment.splice($scope.Attachment.indexOf(item), 1);
                var listFiles = $("#files").data("kendoUpload").getFiles();
                if (listFiles !== null && listFiles.length > 0) {
                    for (var i = 0; i < listFiles.length; i++) {
                        if (listFiles[i].name === item.FileName) {
                            $("#files").data("kendoUpload").removeFileByUid(listFiles[i].uid);
                        }
                    }
                }
                $scope.modelAttachment.FileName = STRING_EMPTY;
            };
        }]);

})(window.angular);
