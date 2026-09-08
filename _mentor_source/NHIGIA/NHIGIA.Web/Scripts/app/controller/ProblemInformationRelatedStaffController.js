(function (angular) {
    "use strict";
    hrmApp.controller('ProblemInformationRelatedStaffController', [
        '$scope', 'ProblemInformationRelatedStaffService', 'EmployeeInformationService', 'ListCategoryService', 'filterFilter',
        function ($scope, ProblemInformationRelatedStaffService, EmployeeInformationService, ListCategoryService, filterFilter) {

            $scope.IsSaveProblemInformationRelatedStaff = false;
            $scope.CheckValidate = false;
            $scope.ListIsCheck = [];

            $scope.ProblemInformationIdTemp = $scope.$parent.IdTemp;

            $scope.modelProblemInformationRelatedStaff = {};

            // BindDataToDropdownlist
            function BindDataToDropdownlist() {
                ListCategoryService.GetDataForDropdown(InjuryCondition).then(function (response) {
                    $scope.InjuryConditionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.InjuryConditionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(CompensationStatus).then(function (response) {
                    $scope.ProcessingStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ProcessingStatusDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(KindOfDecision).then(function (response) {
                    $scope.KindOfDecisionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.KindOfDecisionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(FormsProcessing).then(function (response) {
                    $scope.FormsProcessingDropdownlist = [];
                    if (response.status === 200) {
                        $scope.FormsProcessingDropdownlist = response.data;
                    }
                });
            };
            BindDataToDropdownlist();

            // ShowPopupProblemInformationRelatedStaff
            $scope.ShowPopupProblemInformationRelatedStaff = function (e, id) {
                $scope.IsSaveProblemInformationRelatedStaff = false;
                var valueClick = e;
                $("#btnSaveProblemInformationRelatedStaff").prop("disabled", false);
                $("#btnCloseProblemInformationRelatedStaff").prop("disabled", false);

                onResetValueCheckBox();

                $scope.modelProblemInformationRelatedStaff = {
                    Id: 0,
                    ListEmployeeId: [],
                    ListProblemInformationTrackEmployeeCompensation: [],
                    ProblemInformationId: STRING_EMPTY,
                    EmployeeId: STRING_EMPTY,
                    DescribeTheRelationship: STRING_EMPTY,
                    TotalNumberOfDaysOffDueToOccupationalAccidents: 0,
                    InjuryConditionId: STRING_EMPTY,
                    ProcessingStatusId: STRING_EMPTY,
                    HavePassedLaborSafetyTraining: true,
                    DecisionNumber: STRING_EMPTY,
                    DecisionDate: STRING_EMPTY,
                    KindOfDecisionId: STRING_EMPTY,
                    EffectiveDate: STRING_EMPTY,
                    FormsProcessingId: STRING_EMPTY,
                    TheDecisionId: STRING_EMPTY,
                    CitationOfContent: STRING_EMPTY,
                    EmployeeCode: STRING_EMPTY,
                    EmployeeName: STRING_EMPTY,
                    JobPositionName: STRING_EMPTY,
                    WorkUnitName: STRING_EMPTY,
                    TheDecisionAutoComplete: STRING_EMPTY,
                    TheDecisionTemp: STRING_EMPTY,
                    AmountMoneyAdd: 0,
                    PayDayAdd: STRING_EMPTY,
                    SourceCompensationAdd: STRING_EMPTY,
                    AmountMoneyAdd1: 0
                };

                var windowProblemInformationRelatedStaff = $("#KenWindownProblemInformationRelatedStaff").kendoWindow({
                    actions: ["Close"],
                    draggable: true,
                    modal: true,
                    pinned: false,
                    position: {
                        top: 15
                    },
                    resizable: false,
                    width: "60%"
                }).data('kendoWindow');

                switch (valueClick) {
                    case "ADD_PROBLEMINFORMATIONRELATEDSTAFF":
                        loadingPopUp();
                        windowProblemInformationRelatedStaff.title("Chọn nhân viên liên quan");
                        RefreshKendoGrid("TableProblemInformationRelatedStaff");
                        windowProblemInformationRelatedStaff.open();
                        windowProblemInformationRelatedStaff.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT_PROBLEMINFORMATIONRELATEDSTAFF":
                        $scope.CheckValidate = true;
                        $("#btnSaveProblemInformationRelatedStaffEdit").prop("disabled", false);
                        $("#btnCloseProblemInformationRelatedStaffEdit").prop("disabled", false);
                        var windowKenWindownProblemInformationRelatedStaffEdit = $("#KenWindownProblemInformationRelatedStaffEdit").kendoWindow({
                            actions: ["Close"],
                            draggable: true,
                            modal: true,
                            pinned: false,
                            position: {
                                top: 15
                            },
                            resizable: false,
                            width: "60%"
                        }).data('kendoWindow');
                        loadingPopUp();
                        ProblemInformationRelatedStaffService.GetAllProblemInformationRelatedStaff(id, $scope.ProblemInformationIdTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelProblemInformationRelatedStaff.Id = id;
                                $scope.modelProblemInformationRelatedStaff.ProblemInformationId = $scope.ProblemInformationIdTemp;
                                $scope.modelProblemInformationRelatedStaff.EmployeeId = response.data[0].EmployeeId;
                                $scope.modelProblemInformationRelatedStaff.DescribeTheRelationship = response.data[0].DescribeTheRelationship;
                                $scope.modelProblemInformationRelatedStaff.TotalNumberOfDaysOffDueToOccupationalAccidents = response.data[0].TotalNumberOfDaysOffDueToOccupationalAccidents;
                                $scope.modelProblemInformationRelatedStaff.InjuryConditionId = response.data[0].InjuryConditionId;
                                $scope.modelProblemInformationRelatedStaff.ProcessingStatusId = response.data[0].ProcessingStatusId;
                                $scope.modelProblemInformationRelatedStaff.HavePassedLaborSafetyTraining = response.data[0].HavePassedLaborSafetyTraining;
                                $scope.modelProblemInformationRelatedStaff.DecisionNumber = response.data[0].DecisionNumber;
                                $scope.modelProblemInformationRelatedStaff.DecisionDate = kendo.parseDate(response.data[0].DecisionDate, DATE_FORMAT);
                                $scope.modelProblemInformationRelatedStaff.KindOfDecisionId = response.data[0].KindOfDecisionId;
                                $scope.modelProblemInformationRelatedStaff.EffectiveDate = kendo.parseDate(response.data[0].EffectiveDate, DATE_FORMAT);
                                $scope.modelProblemInformationRelatedStaff.FormsProcessingId = response.data[0].FormsProcessingId;
                                $scope.modelProblemInformationRelatedStaff.TheDecisionId = response.data[0].TheDecisionId;
                                $scope.modelProblemInformationRelatedStaff.CitationOfContent = response.data[0].CitationOfContent;
                                $scope.modelProblemInformationRelatedStaff.EmployeeCode = response.data[0].EmployeeCode;
                                $scope.modelProblemInformationRelatedStaff.EmployeeName = response.data[0].EmployeeName;
                                $scope.modelProblemInformationRelatedStaff.JobPositionName = response.data[0].JobPositionName;
                                $scope.modelProblemInformationRelatedStaff.WorkUnitName = response.data[0].WorkUnitName;
                                $scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete = response.data[0].TheDecisionAutoComplete;
                                $scope.modelProblemInformationRelatedStaff.TheDecisionTemp = response.data[0].TheDecisionAutoComplete;
                                $(".k-i-plus").addClass('k-i-plus-circle');
                                $(".k-i-edit").addClass('k-i-pencil');
                                $(".k-i-close").addClass('k-i-x');

                                // GetAllProblemInformationTrackEmployeeCompensation
                                // Theo dõi nhân viên thực hiện bồi thường = 1
                                ProblemInformationRelatedStaffService.GetAllProblemInformationTrackEmployeeCompensation($scope.ProblemInformationIdTemp, response.data[0].EmployeeId, 1).then(function (response) {
                                    $scope.GetAllProblemInformationTrackEmployeeCompensation = [];
                                    if (response.status === 200) {
                                        $scope.GetAllProblemInformationTrackEmployeeCompensation = response.data;
                                    }
                                });
                                // Các khoản nhân viên được bồi thường = 2
                                ProblemInformationRelatedStaffService.GetAllProblemInformationTrackEmployeeCompensation($scope.ProblemInformationIdTemp, response.data[0].EmployeeId, 2).then(function (response) {
                                    $scope.GetAllEmployeeCompensation = [];
                                    if (response.status === 200) {
                                        $scope.GetAllEmployeeCompensation = response.data;
                                    }
                                });

                                windowKenWindownProblemInformationRelatedStaffEdit.title("Sửa dữ liệu");
                                windowKenWindownProblemInformationRelatedStaffEdit.open();
                                windowKenWindownProblemInformationRelatedStaffEdit.center();
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE_PROBLEMINFORMATIONRELATEDSTAFF":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelProblemInformationRelatedStaff);
                                data.Id = id;
                                ProblemInformationRelatedStaffService.DeleteProblemInformationRelatedStaff(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("RELATED_STAFF");
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

            // onResetValueCheckBox
            function onResetValueCheckBox() {
                $("#checkAll").prop("checked", false);
                $scope.ListIsCheck = [];
            }

            // Designer gird
            $scope.SectionProblemInformationRelatedStaff = {
                dataSource: EmployeeInformationService.GetEmployee($scope.ProblemInformationIdTemp, "ProblemInformationRelatedStaff"),
                resizeable: true,
                autoBind: true,
                sortable: {
                    mode: SINGLE,
                    allowUnsort: true
                },
                noRecords: true,
                messages: {
                    noRecords: NORECORDS
                },
                filterable: {
                    extra: false,
                    operators: {
                        string: {
                            contains: CONTENT_FILTER
                        }
                    },
                    messages: {
                        info: STRING_EMPTY,
                        filter: FILTER,
                        clear: CLEAR,
                        search: STRING_EMPTY,
                        selectedItemsFormat: SELECTEDITEMSFORMAT
                    },
                    mode: ROW
                },
                filter: function (e) {
                    onResetValueCheckBox();
                },
                sort: function (e) {
                    onResetValueCheckBox();
                },
                dataBinding: function (e) {
                    onResetValueCheckBox();
                },
                scrollable: true,
                pageable: {
                    refresh: false,
                    input: false,
                    pageSizes: [10, 50, 100],
                    buttonCount: 5,
                    messages: {
                        itemsPerPage: ITEMSPERPAGE,
                        display: DISPLAY,
                        empty: EMPTY
                    },
                    change: function (e) {
                        onResetValueCheckBox();
                    }
                },
                columns: [
                    {
                        headerTemplate: "<div class='demo-checkbox'><input type='checkbox' id='checkAll' class='filled-in chk-col-blue' name='checkAll' ng-click='selectAllRow($event)' /> <label for='checkAll'></label></div>",
                        template: function (dataItem) {
                            return "<div class='demo-checkbox'><input type='checkbox' id='checkbox" + dataItem.Id + "' class='filled-in chk-col-blue' ng-checked='dataItem.IsCheck' ng-click='selectRow($event,dataItem)'><label class='m-l-9' for='checkbox" + dataItem.Id + "'></label></div>";
                        },
                        width: "50px"
                    },
                    { hidden: true, field: "Id" },
                    { hidden: true, field: "IsCheck" },
                    {
                        field: "EmployeeCode", title: "Mã nhân viên", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EmployeeName", title: "Họ và tên", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "JobPositionName", title: "Vị trí công việc", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "WorkUnitName", title: "Đơn vị công tác", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    }
                ]
            };

            // CloseProblemInformationRelatedStaff
            $scope.CloseProblemInformationRelatedStaff = function () {
                $("#KenWindownProblemInformationRelatedStaff").closest(".k-window-content").data("kendoWindow").close();
            };

            // CloseProblemInformationRelatedStaffEdit
            $scope.CloseProblemInformationRelatedStaffEdit = function () {
                $("#KenWindownProblemInformationRelatedStaffEdit").closest(".k-window-content").data("kendoWindow").close();
            };

            //on click of the checkbox:
            $scope.selectRow = function ($event, dataItem) {
                var checked = $event.currentTarget.checked;
                dataItem.IsCheck = checked;

                $scope.ListIsCheck.push({
                    EmployeeId: dataItem.Id,
                    ProblemInformationId: $scope.ProblemInformationIdTemp
                });

                if (dataItem.IsCheck === false) {
                    $scope.ListIsCheck = $scope.ListIsCheck.filter(item => item.EmployeeId !== dataItem.Id);
                }
            };

            //on click of the all checkbox:
            $scope.selectAllRow = function ($event) {
                var checked = $event.currentTarget.checked;
                var grid = $("#TableProblemInformationRelatedStaff").data("kendoGrid");
                var items = grid.items();
                items.each(function () {
                    var dataItem = grid.dataItem(this);
                    if (dataItem.IsCheck != checked) {
                        dataItem.IsCheck = checked;
                        dataItem.dirty = true;
                        $scope.ListIsCheck.push({
                            EmployeeId: dataItem.Id,
                            ProblemInformationId: $scope.ProblemInformationIdTemp
                        });
                        if (dataItem.IsCheck === false) {
                            $scope.ListIsCheck = [];
                        }
                    }
                });
            };

            // SaveProblemInformationRelatedStaff
            $scope.SaveProblemInformationRelatedStaff = function () {
                $scope.IsSaveProblemInformationRelatedStaff = true;

                if ($scope.ListIsCheck.length === 0) {
                    window.alert("Bạn cần phải chọn ít nhất 1 nhân viên");
                    return;
                }

                var data = angular.copy($scope.modelProblemInformationRelatedStaff);
                data.ListEmployeeId = $scope.ListIsCheck;

                loadingPopUp();
                $("#btnSaveProblemInformationRelatedStaff").prop("disabled", true);
                $("#btnCloseProblemInformationRelatedStaff").prop("disabled", true);
                ProblemInformationRelatedStaffService.SaveProblemInformationRelatedStaff(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownProblemInformationRelatedStaff").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("RELATED_STAFF");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownProblemInformationRelatedStaff").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // Load datasource for TheDecisionId autocomplete box
            $scope.GetEmployeeForTheDecision = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item TheDecisionId autocomplete box
            $scope.onSelectTheDecisionId = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForTheDecision.data()[index];
                $scope.modelProblemInformationRelatedStaff.TheDecisionId = itemSelected.Id;
                $scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#TheDecisionId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item TheDecisionId autocomplete box
            $scope.onChangeTheDecisionId = function () {
                if ($scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete === $scope.modelProblemInformationRelatedStaff.TheDecisionTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForTheDecision.data().forEach(function (item) {
                    if ($scope.modelProblemInformationRelatedStaff.TheDecisionId === item.Id) {
                        $scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelProblemInformationRelatedStaff.TheDecisionId = STRING_EMPTY;
                    $scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete = STRING_EMPTY;
                }
            };

            // onChangeTheDecisionIdValue
            $scope.onChangeTheDecisionIdValue = function () {
                if ($scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete === STRING_EMPTY) {
                    $scope.modelProblemInformationRelatedStaff.TheDecisionId = STRING_EMPTY;
                    $scope.modelProblemInformationRelatedStaff.TheDecisionAutoComplete = STRING_EMPTY;
                }
            };

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorProcessingStatusId = false;
            }

            // onChangeValueEdit
            $scope.onChangeValueEdit = function (e) {
                switch (e) {
                    case "ProcessingStatusId":
                        if ($scope.modelProblemInformationRelatedStaff.ProcessingStatusId !== STRING_EMPTY) {
                            $scope.showHasErrorProcessingStatusId = false;
                        } else {
                            $scope.showHasErrorProcessingStatusId = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveProblemInformationRelatedStaffEdit
            $scope.SaveProblemInformationRelatedStaffEdit = function () {

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelProblemInformationRelatedStaff.ProcessingStatusId || $scope.modelProblemInformationRelatedStaff.ProcessingStatusId === STRING_EMPTY) {
                        $scope.showHasErrorProcessingStatusId = true;
                        return;
                    }

                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.modelProblemInformationRelatedStaff);
                data.DecisionDate = kendo.parseDate($scope.modelProblemInformationRelatedStaff.DecisionDate, DATE_FORMAT);
                data.EffectiveDate = kendo.parseDate($scope.modelProblemInformationRelatedStaff.EffectiveDate, DATE_FORMAT);
                data.ListProblemInformationTrackEmployeeCompensation = filterFilter($scope.GetAllProblemInformationTrackEmployeeCompensation, { IsEdit: 1 });
                data.ListProblemInformationTrackEmployeeCompensation = filterFilter($scope.GetAllEmployeeCompensation, { IsEdit: 1 });

                debugger
                loadingPopUp();
                $("#btnSaveProblemInformationRelatedStaffEdit").prop("disabled", true);
                $("#btnCloseProblemInformationRelatedStaffEdit").prop("disabled", true);
                ProblemInformationRelatedStaffService.SaveOnlyProblemInformationRelatedStaff(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownProblemInformationRelatedStaffEdit").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("RELATED_STAFF");

                        ProblemInformationRelatedStaffService.SaveProblemInformationTrackEmployeeCompensation(data).then(function success(response) {
                            if (response.data.status === 200) {
                                stopLoadingPopUp();
                            } else {
                                stopLoadingPopUp();
                            }
                        });

                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownProblemInformationRelatedStaffEdit").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // Handle logic edit inline TrackEmployeeCompensation
            $scope.editablerowTrackEmployeeCompensation = STRING_EMPTY;

            $scope.editContentTrackEmployeeCompensation = function (item) {
                item.IsEdit = 1;
                $scope.editablerowTrackEmployeeCompensation = angular.copy(item);
            };

            $scope.deleteContentTrackEmployeeCompensation = function (item, indx) {
                if (confirm("Bạn có chắc chắn muốn xóa bản ghi này không?")) {
                    $scope.GetAllProblemInformationTrackEmployeeCompensation[indx].IsEdit = 1;
                    $scope.GetAllProblemInformationTrackEmployeeCompensation[indx].IsDeleted = true;
                    $(".ProblemInformationTrackEmployeeCompensation_" + item.AmountMoney + "_" + item.Id).addClass("hide");

                    $scope.GetAllProblemInformationTrackEmployeeCompensationTemp = filterFilter($scope.GetAllProblemInformationTrackEmployeeCompensation, { IsDeleted: false });
                    if ($scope.GetAllProblemInformationTrackEmployeeCompensationTemp.length === 0) {
                        $(".ShowDeleteClient").removeClass("hide");
                    }
                }
            };

            $scope.loadTemplateTrackEmployeeCompensation = function (item) {
                if (item.Id === $scope.editablerowTrackEmployeeCompensation.Id && item.IsDeleted === false) {
                    return 'editTrackEmployeeCompensation';
                }
                else {
                    return 'displayTrackEmployeeCompensation';
                }
            };

            $scope.saveDataTrackEmployeeCompensation = function (indx) {
                $scope.editablerowTrackEmployeeCompensation.PayDayString = kendo.parseDate($scope.editablerowTrackEmployeeCompensation.PayDay, DATE_FORMAT);
                $scope.editablerowTrackEmployeeCompensation.PayDay = kendo.parseDate($scope.editablerowTrackEmployeeCompensation.PayDay, DATE_FORMAT);
                $scope.GetAllProblemInformationTrackEmployeeCompensation[indx] = angular.copy($scope.editablerowTrackEmployeeCompensation);
                $scope.resetTrackEmployeeCompensation();
            };

            $scope.resetTrackEmployeeCompensation = function () {
                $scope.editablerowTrackEmployeeCompensation = [];
            };

            $scope.insertDataTrackEmployeeCompensation = function () {
                var obj = {
                    Id: 0,
                    ProblemInformationId: $scope.ProblemInformationIdTemp,
                    EmployeeId: $scope.modelProblemInformationRelatedStaff.EmployeeId,
                    AmountMoney: $scope.modelProblemInformationRelatedStaff.AmountMoneyAdd,
                    PayDay: kendo.parseDate($scope.modelProblemInformationRelatedStaff.PayDayAdd, DATE_FORMAT),
                    PayDayString: kendo.parseDate($scope.modelProblemInformationRelatedStaff.PayDayAdd, DATE_FORMAT),
                    SourceCompensation: STRING_EMPTY,
                    Type: 1,
                    IsEdit: 1,
                    IsDeleted: false
                };

                for (var i = 0; i < $scope.GetAllProblemInformationTrackEmployeeCompensation.length; i++) {
                    if ($scope.GetAllProblemInformationTrackEmployeeCompensation[i].AmountMoney == $scope.modelProblemInformationRelatedStaff.AmountMoneyAdd
                        && $scope.GetAllProblemInformationTrackEmployeeCompensation[i].IsDeleted === false) {
                        window.alert(MESSAGE_ERROR_DUPLICATE_DATA);
                        return;
                    }
                };

                $scope.GetAllProblemInformationTrackEmployeeCompensation.push(obj);
                $scope.modelProblemInformationRelatedStaff.AmountMoneyAdd = 0;
                $scope.modelProblemInformationRelatedStaff.PayDayAdd = STRING_EMPTY;

                if ($scope.GetAllProblemInformationTrackEmployeeCompensation.length > 0) {
                    $(".ShowDeleteClient").addClass("hide");
                };
            };

            // Handle logic edit inline EmployeeCompensation
            $scope.editablerowEmployeeCompensation = STRING_EMPTY;

            $scope.editContentEmployeeCompensation = function (item) {
                item.IsEdit = 1;
                $scope.editablerowEmployeeCompensation = angular.copy(item);
            };

            $scope.deleteContentEmployeeCompensation = function (item, indx) {
                if (confirm("Bạn có chắc chắn muốn xóa bản ghi này không?")) {
                    $scope.GetAllEmployeeCompensation[indx].IsEdit = 1;
                    $scope.GetAllEmployeeCompensation[indx].IsDeleted = true;
                    $(".ProblemInformationEmployeeCompensation_" + item.SourceCompensation + "_" + item.Id).addClass("hide");

                    $scope.GetAllEmployeeCompensationTemp = filterFilter($scope.GetAllEmployeeCompensation, { IsDeleted: false });
                    if ($scope.GetAllEmployeeCompensationTemp.length === 0) {
                        $(".ShowDeleteClient1").removeClass("hide");
                    }
                }
            };

            $scope.loadTemplateEmployeeCompensation = function (item) {
                if (item.Id === $scope.editablerowEmployeeCompensation.Id && item.IsDeleted === false) {
                    return 'editEmployeeCompensation';
                }
                else {
                    return 'displayEmployeeCompensation';
                }
            };

            $scope.saveDataEmployeeCompensation = function (indx) {
                $scope.GetAllEmployeeCompensation[indx] = angular.copy($scope.editablerowEmployeeCompensation);
                $scope.resetEmployeeCompensation();
            };

            $scope.resetEmployeeCompensation = function () {
                $scope.editablerowEmployeeCompensation = [];
            };

            $scope.insertDataEmployeeCompensation = function () {
                var obj = {
                    Id: 0,
                    ProblemInformationId: $scope.ProblemInformationIdTemp,
                    EmployeeId: $scope.modelProblemInformationRelatedStaff.EmployeeId,
                    SourceCompensation: $scope.modelProblemInformationRelatedStaff.SourceCompensationAdd,
                    AmountMoney: $scope.modelProblemInformationRelatedStaff.AmountMoneyAdd1,
                    Type: 2,
                    IsEdit: 1,
                    IsDeleted: false
                };

                for (var i = 0; i < $scope.GetAllEmployeeCompensation.length; i++) {
                    if ($scope.GetAllEmployeeCompensation[i].SourceCompensation == $scope.modelProblemInformationRelatedStaff.SourceCompensationAdd
                        && $scope.GetAllEmployeeCompensation[i].IsDeleted === false) {
                        window.alert(MESSAGE_ERROR_DUPLICATE_DATA);
                        return;
                    }
                };

                $scope.GetAllEmployeeCompensation.push(obj);
                $scope.modelProblemInformationRelatedStaff.SourceCompensationAdd = STRING_EMPTY;
                $scope.modelProblemInformationRelatedStaff.AmountMoneyAdd1 = 0;

                if ($scope.GetAllEmployeeCompensation.length > 0) {
                    $(".ShowDeleteClient1").addClass("hide");
                };
            };

        }]);

})(window.angular);
