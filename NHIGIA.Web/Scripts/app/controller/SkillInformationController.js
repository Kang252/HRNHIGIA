(function (angular) {
    "use strict";
    hrmApp.controller('SkillInformationController', [
        '$scope', 'SkillInformationService',
        function ($scope, SkillInformationService) {

            // Define
            $scope.IsSaveSkill = false;
            $scope.CheckValidate = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorSkillName = false;
            }

            // ShowPopupSkill
            $scope.ShowPopupSkill = function (e, id) {
                $scope.IsSaveSkill = false;
                $scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveSkill").prop("disabled", false);
                $("#btnCloseSkill").prop("disabled", false);

                $scope.modelSkill = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    SkillName: STRING_EMPTY,
                    SkillGroupId: STRING_EMPTY,
                    SkillLevelId: STRING_EMPTY,
                    Note: STRING_EMPTY
                };

                var windowSkill = $("#KenWindownSkill").kendoWindow({
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
                    case "ADD_SKILL":
                        loadingPopUp();
                        windowSkill.title("Thêm mới");
                        windowSkill.open();
                        windowSkill.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_SKILL":
                        loadingPopUp();
                        SkillInformationService.GetAllSkillInformation(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelSkill.Id = id;
                                $scope.modelSkill.EmployeeId = $scope.EmployeeTemp;
                                $scope.modelSkill.SkillName = response.data[0].SkillName;
                                $scope.modelSkill.SkillGroupId = response.data[0].SkillGroupId;
                                $scope.modelSkill.SkillLevelId = response.data[0].SkillLevelId;
                                $scope.modelSkill.Note = response.data[0].Note;
                                windowSkill.title("Sửa dữ liệu");
                                windowSkill.open();
                                windowSkill.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_SKILL":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelSkill);
                                data.Id = id;
                                SkillInformationService.DeleteSkillInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("SKILL");
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

            // CloseSkill
            $scope.CloseSkill = function () {
                $scope.IsSaveSkill = false;
                onShowMessageValidate();
                $("#KenWindownSkill").closest(".k-window-content").data("kendoWindow").close();
            };

            // onChangeValueSkill
            $scope.onChangeValueSkill = function (e) {
                switch (e) {
                    case "SkillName":
                        if ($scope.modelSkill.SkillName !== STRING_EMPTY) {
                            $scope.showHasErrorSkillName = false;
                        } else {
                            $scope.showHasErrorSkillName = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveSkill
            $scope.SaveSkill = function (form) {
                $scope.IsSaveSkill = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelSkill.SkillName || $scope.modelSkill.SkillName === STRING_EMPTY) {
                        $scope.showHasErrorSkillName = true;
                    }
                    if ($scope.showHasErrorSkillName === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelSkill);
                data.EmployeeId = $scope.EmployeeTemp;

                loadingPopUp();
                $("#btnSaveSkill").prop("disabled", true);
                $("#btnCloseSkill").prop("disabled", true);
                SkillInformationService.SaveSkillInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownSkill").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("SKILL");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownSkill").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };
        }]);

})(window.angular);
