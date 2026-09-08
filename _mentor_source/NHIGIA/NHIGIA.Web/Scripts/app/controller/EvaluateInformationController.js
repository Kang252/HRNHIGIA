(function (angular) {
    "use strict";
    hrmApp.controller('EvaluateInformationController', [
        '$scope', 'EvaluateService',
        function ($scope, EvaluateService) {

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // ShowPopupEvaluate
            $scope.ShowPopupEvaluate = function (e, id) {
                $("#btnSaveEvaluateForm").addClass('hidden');
                $("#EvaluateForm1").prop("disabled", true);
                $("#EvaluateForm2").prop("disabled", true);
                $("#EvaluateForm3").prop("disabled", true);
                $("#EvaluateForm4").prop("disabled", true);
                $("#EvaluateForm5").prop("disabled", true);
                $("#EvaluateForm6").prop("disabled", true);
                $("#EvaluateForm7").prop("disabled", true);
                $("#EvaluateForm8").prop("disabled", true);
                $("#EvaluateForm9").prop("disabled", true);
                $("#EvaluateForm10").prop("disabled", true);
                $("#EvaluateForm11").prop("disabled", true);
                $("#EvaluateForm12").prop("disabled", true);
                $("#EvaluateForm13").prop("disabled", true);
                $("#EvaluateForm14").prop("disabled", true);
                $("#EvaluateForm15").prop("disabled", true);
                $("#EvaluateForm16").prop("disabled", true);
                $("#EvaluateForm17").prop("disabled", true);
                $("#EvaluateForm18").prop("disabled", true);
                $("#EvaluateForm19").prop("disabled", true);
                $("#EvaluateForm20").prop("disabled", true);

                var valueClick = e;

                $scope.modelEvaluateForm = {
                    Id: 0,
                    NameOfAudit: STRING_EMPTY,
                    DateString: STRING_EMPTY,
                    EvaluationPeriodName: STRING_EMPTY,
                    Result: STRING_EMPTY,
                    EvaluateForm1: 0,
                    EvaluateForm2: 0,
                    EvaluateForm3: 0,
                    EvaluateForm4: 0,
                    EvaluateForm5: 0,
                    EvaluateForm6: 0,
                    EvaluateForm7: 0,
                    EvaluateForm8: 0,
                    EvaluateForm9: 0,
                    EvaluateForm10: 0,
                    EvaluateForm11: 0,
                    EvaluateForm12: 0,
                    EvaluateForm13: 0,
                    EvaluateForm14: 0,
                    EvaluateForm15: 0,
                    EvaluateForm16: 0,
                    EvaluateForm17: 0,
                    EvaluateForm18: 0,
                    EvaluateForm19: 0,
                    EvaluateForm20: 0
                };

                var window = $("#KenWindownEvaluateInformation").kendoWindow({
                    actions: ["Close"],
                    draggable: true,
                    modal: true,
                    pinned: false,
                    position: {
                        top: 15
                    },
                    resizable: false,
                    width: "50%",
                    height: 520
                }).data('kendoWindow');

                switch (valueClick) {
                    case "VIEW_EVALUATE":
                        loadingPopUp();
                        EvaluateService.GetEvaluateByEmployee(id, $scope.EmployeeTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelEvaluateForm.Id = id;
                                $scope.modelEvaluateForm.NameOfAudit = response.data[0].NameOfAudit;
                                $scope.modelEvaluateForm.DateString = response.data[0].DateString;
                                $scope.modelEvaluateForm.EvaluationPeriodName = response.data[0].EvaluationPeriodName;
                                $scope.modelEvaluateForm.Result = response.data[0].Result;

                                if (response.data[0].ResultJson && response.data[0].ResultJson !== null) {
                                    var obj = JSON.parse(response.data[0].ResultJson);
                                    $scope.modelEvaluateForm.EvaluateForm1 = obj.EvaluateForm1;
                                    $scope.modelEvaluateForm.EvaluateForm2 = obj.EvaluateForm2;
                                    $scope.modelEvaluateForm.EvaluateForm3 = obj.EvaluateForm3;
                                    $scope.modelEvaluateForm.EvaluateForm4 = obj.EvaluateForm4;
                                    $scope.modelEvaluateForm.EvaluateForm5 = obj.EvaluateForm5;
                                    $scope.modelEvaluateForm.EvaluateForm6 = obj.EvaluateForm6;
                                    $scope.modelEvaluateForm.EvaluateForm7 = obj.EvaluateForm7;
                                    $scope.modelEvaluateForm.EvaluateForm8 = obj.EvaluateForm8;
                                    $scope.modelEvaluateForm.EvaluateForm9 = obj.EvaluateForm9;
                                    $scope.modelEvaluateForm.EvaluateForm10 = obj.EvaluateForm10;
                                    $scope.modelEvaluateForm.EvaluateForm11 = obj.EvaluateForm11;
                                    $scope.modelEvaluateForm.EvaluateForm12 = obj.EvaluateForm12;
                                    $scope.modelEvaluateForm.EvaluateForm13 = obj.EvaluateForm13;
                                    $scope.modelEvaluateForm.EvaluateForm14 = obj.EvaluateForm14;
                                    $scope.modelEvaluateForm.EvaluateForm15 = obj.EvaluateForm15;
                                    $scope.modelEvaluateForm.EvaluateForm16 = obj.EvaluateForm16;
                                    $scope.modelEvaluateForm.EvaluateForm17 = obj.EvaluateForm17;
                                    $scope.modelEvaluateForm.EvaluateForm18 = obj.EvaluateForm18;
                                    $scope.modelEvaluateForm.EvaluateForm19 = obj.EvaluateForm19;
                                    $scope.modelEvaluateForm.EvaluateForm20 = obj.EvaluateForm20;
                                    window.title("Kết quả đánh giá");
                                    window.open();
                                    window.center();
                                    loadingPopUp();
                                } else {
                                    bootbox.alert("<span style='color:green; text-align:justify;'>Chưa có kết quả đánh giá</span>");
                                    loadingPopUp();
                                }
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                loadingPopUp();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            // CloseEvaluateForm
            $scope.CloseEvaluateForm = function () {
                $("#KenWindownEvaluateInformation").closest(".k-window-content").data("kendoWindow").close();
            };

        }]);

})(window.angular);
