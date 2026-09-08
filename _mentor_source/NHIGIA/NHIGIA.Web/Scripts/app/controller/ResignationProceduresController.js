(function (angular) {
    "use strict";
    hrmApp.controller('ResignationProceduresController', [
        '$scope', 'ResignationProceduresService', 'ListCategoryService', '$window', 'QuitInformationService', 'AssetInformationService',
        'ResignationProceduresEmployeeDebtService', 'EmployeeInformationService',
        function ($scope, ResignationProceduresService, ListCategoryService, $window, QuitInformationService, AssetInformationService,
            ResignationProceduresEmployeeDebtService, EmployeeInformationService) {

            $(document).ready(function () {
                $('.procedure_active').removeClass('active');
                $('.procedure_active').addClass('active');
                $('.procedure_toggle').removeClass('toggled');
                $('.procedure_toggle').addClass('toggled');
                $('.procedure_bussiness_display').removeClass('display_block');
                $('.procedure_bussiness_display').addClass('display_block');
                $('.resignation_procedures_active').removeClass('active');
                $('.resignation_procedures_active').addClass('active');
                $('.resignation_procedures_toggle').removeClass('toggled');
                $('.resignation_procedures_toggle').addClass('toggled');
            });

            // Define
            $scope.IsSave = false;

            $scope.model = {};

            $scope.ShowList = true;
            $scope.ShowInfo = false;

            $scope.IdTemp = 0;
            $scope.EmployeeIdTemp = 0;
            $scope.DayOffTemp = STRING_EMPTY;

            $scope.CheckDayOff = false;
            $scope.MessageCheckDayOff = "Ngày nghỉ việc phải lớn hơn hoặc bằng Ngày quyết định";

            // BindDataToDropdownlist
            function BindDataToDropdownlist() {
                ListCategoryService.GetStatusForDropdown(NameOfTheDebt).then(function (response) {
                    $scope.NameOfTheDebtDropdownlist = [];
                    if (response.status === 200) {
                        $scope.NameOfTheDebtDropdownlist = response.data;
                    }
                });
            };

            // onChange
            function onChange(arg) {
                $scope.IdTemp = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.EmployeeIdTemp = arg.sender.dataItem(arg.sender.select()).EmployeeId;
                $scope.DayOffTemp = arg.sender.dataItem(arg.sender.select()).DayOff;
                $("#ButtonFinishProcedures").prop("disabled", false);
                $("#ButtonResignationUpdate").prop("disabled", false);
                $("#ButtonDelete").prop("disabled", false);
                checkShowHideForButton($scope.DayOffTemp);

            }

            function checkShowHideForButton(e) {
                if (e === STRING_EMPTY || e === null) {
                    $('.add_hide').removeClass('hide');
                    $('.add_hide').addClass('display-inline-block');
                    $('.add_show').removeClass('display-inline-block');
                    $('.add_show').addClass('hide');
                }
                else {
                    $('.add_hide').removeClass('display-inline-block');
                    $('.add_hide').addClass('hide');
                    $('.add_show').removeClass('hide');
                    $('.add_show').addClass('display-inline-block');
                }
            }

            // onDisable
            function onDisable() {
                $("#ButtonFinishProcedures").prop("disabled", true);
                $("#ButtonResignationUpdate").prop("disabled", true);
                $("#ButtonDelete").prop("disabled", true);
                $scope.IdTemp = 0;
            }

            // Designer gird
            $scope.Section = {
                dataSource: ResignationProceduresService.GetResignationProcedures(),
                resizeable: true,
                autoBind: true,
                sortable: {
                    mode: SINGLE,
                    allowUnsort: true
                },
                change: onChange,
                selectable: true,
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
                    onDisable();
                },
                sort: function (e) {
                    onDisable();
                },
                dataBinding: function (e) {
                    onDisable();
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
                        onDisable();
                    },
                },
                columns: [
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
                    },
                    {
                        field: "DayOff", title: "Ngày nghỉ việc", width: "150px",
                        template: "<span>#= (DayOff == null) ? '' : kendo.toString(kendo.parseDate(DayOff, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    }
                ]
            };

            $scope.LoadGetGeneralInformationForResignationProcedures = function (e) {
                ResignationProceduresService.GetGeneralInformationForResignationProcedures(e).then(function (response) {
                    if (response.status === 200 && response.data.Success === true) {
                        $scope.model.Id = $scope.IdTemp;
                        $scope.model.EmployeeId = response.data.Data.EmployeeId;
                        $scope.model.EmployeeCode = response.data.Data.EmployeeCode;
                        $scope.model.EmployeeName = response.data.Data.EmployeeName;
                        $scope.model.JobPositionName = response.data.Data.JobPositionName;
                        $scope.model.WorkUnitName = response.data.Data.WorkUnitName;
                        $scope.model.ProbationDayString = response.data.Data.ProbationDayString;
                        $scope.model.MobilePhone = response.data.Data.MobilePhone;
                        $scope.model.CompanyEmail = response.data.Data.CompanyEmail;
                        $scope.model.SomeContracts = response.data.Data.SomeContracts;
                        $scope.model.ContractTypeName = response.data.Data.ContractTypeName;
                        $scope.model.EffectiveDateString = response.data.Data.EffectiveDateString;
                        $scope.model.ExpirationDateString = response.data.Data.ExpirationDateString;
                        $scope.model.DayOfftring = response.data.Data.DayOfftring;
                        $scope.model.ReviewerName = response.data.Data.ReviewerName;
                        $scope.model.ReasonForRest = response.data.Data.ReasonForRest;
                        $scope.model.Comments = response.data.Data.Comments;
                        $scope.model.DecisionNumber = response.data.Data.DecisionNumber;
                        $scope.model.Note = response.data.Data.Note;
                        stopLoadingProfilePage();
                    }
                });
            };

            // ShowPopup
            $scope.ShowPopup = function (e) {
                $scope.IsSave = false;
                var valueClick = e;

                $scope.model = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    EmployeeCode: STRING_EMPTY,
                    EmployeeName: STRING_EMPTY,
                    JobPositionName: STRING_EMPTY,
                    WorkUnitName: STRING_EMPTY,
                    ProbationDayString: STRING_EMPTY,
                    MobilePhone: STRING_EMPTY,
                    CompanyEmail: STRING_EMPTY,
                    SomeContracts: STRING_EMPTY,
                    ContractTypeName: STRING_EMPTY,
                    EffectiveDateString: STRING_EMPTY,
                    ExpirationDateString: STRING_EMPTY,
                    DayOfftring: STRING_EMPTY,
                    ReviewerName: STRING_EMPTY,
                    ReasonForRest: STRING_EMPTY,
                    Comments: STRING_EMPTY,
                    DecisionNumber: STRING_EMPTY,
                    Note: STRING_EMPTY,
                    DecisionNumber: STRING_EMPTY,
                    DecisionDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    DayOff: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ReviewerId: STRING_EMPTY,
                    ReviewerAutoComplete: STRING_EMPTY,
                    ReasonForRest: STRING_EMPTY,
                    Comments: STRING_EMPTY
                };

                switch (valueClick) {
                    case "FINISH_PROCEDURES":
                        loadingProfilePage();
                        if ($scope.IdTemp === 0) {
                            bootbox.alert("<span style='color:red; text-align:justify;'>" + CANNOT_FIND_ANY_WITH_GIVEN_ID + "</span>");
                            $scope.IdTemp = 0;
                            stopLoadingProfilePage();
                        } else {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                            $scope.LoadGetGeneralInformationForResignationProcedures($scope.IdTemp);
                        }
                        break;
                    case "RESIGNATION_UPDATE":
                        loadingProfilePage();
                        if ($scope.IdTemp === 0) {
                            bootbox.alert("<span style='color:red; text-align:justify;'>" + CANNOT_FIND_ANY_WITH_GIVEN_ID + "</span>");
                            $scope.IdTemp = 0;
                            stopLoadingProfilePage();
                        } else {
                            if ($scope.DayOffTemp === STRING_EMPTY || $scope.DayOffTemp === null) {
                                $("#btnSaveResignationUpdate").prop("disabled", false);
                                $("#btnCloseResignationUpdate").prop("disabled", false);
                                var windowResignationUpdate = $("#KenWindownResignationUpdate").kendoWindow({
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
                                windowResignationUpdate.title("Cập nhật thôi việc");
                                windowResignationUpdate.open();
                                windowResignationUpdate.center();
                                stopLoadingProfilePage();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>Thủ tục đã kết thúc, vui lòng chọn thủ tục khác</span>");
                                stopLoadingProfilePage();
                            }

                        }
                        break;
                    case "DELETE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.model);
                                data.Id = $scope.IdTemp;
                                data.EmployeeId = $scope.EmployeeIdTemp;
                                ResignationProceduresService.DeleteResignationProcedures(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        RefreshKendoGrid("Table");
                                        stopLoadingPopUp();
                                    } else {
                                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                        stopLoadingPopUp();
                                    }
                                });
                            }
                        });
                        break;
                    case "CANCEL":
                        loadingProfilePage();
                        $window.location.reload();
                        stopLoadingProfilePage();
                        break;
                    case "REFRESH":
                        loadingProfilePage();
                        $scope.DayOffTemp = STRING_EMPTY;
                        RefreshKendoGrid("Table");
                        checkShowHideForButton($scope.DayOffTemp);
                        stopLoadingProfilePage();
                        break;
                    default:
                        break;
                }
            };

            // Upsite
            $scope.Upsite = function () {
                $window.scrollTo(0, 0);
            };

            // Save
            $scope.Save = function (form) {
                $scope.IsSave = true;

                var data = angular.copy($scope.model);
                data.Id = $scope.IdTemp;
                data.EmployeeId = $scope.EmployeeIdTemp;
                data.DecisionDate = kendo.parseDate($scope.model.DecisionDate, DATE_FORMAT);
                data.DayOff = kendo.parseDate($scope.model.DayOff, DATE_FORMAT);

                // Check validate DecisionDate < DayOff
                if (data.DecisionDate != null && data.DayOff != null && data.DecisionDate.getTime() > data.DayOff.getTime()) {
                    $scope.CheckDayOff = true;
                    return;
                } else {
                    $scope.CheckDayOff = false;
                }

                loadingPopUp();
                $("#btnSaveResignationUpdate").prop("disabled", true);
                $("#btnCloseResignationUpdate").prop("disabled", true);
                ResignationProceduresService.SaveResignationProcedures(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownResignationUpdate").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        RefreshKendoGrid("Table");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownResignationUpdate").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // ClickInformation
            $scope.ClickInformation = function (e) {
                switch (e) {
                    case "GENERAL_INFORMATION":
                        loadingProfilePage();
                        $scope.LoadGetGeneralInformationForResignationProcedures($scope.IdTemp);
                        break;
                    case "PROCEDURE":
                        loadingProfilePage();
                        QuitInformationService.GetAllQuitInformation(0, $scope.model.EmployeeId).then(function (response) {
                            $scope.GetAllQuitInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllQuitInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "PROPERTY_HANDED_OVER":
                        loadingProfilePage();
                        AssetInformationService.GetAllAssetInformation(0, $scope.model.EmployeeId).then(function (response) {
                            $scope.GetAllAssetInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllAssetInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "EMPLOYEE_DEBT":
                        loadingProfilePage();
                        checkShowHideForButton($scope.DayOffTemp);
                        BindDataToDropdownlist();
                        ResignationProceduresEmployeeDebtService.GetAllResignationProceduresEmployeeDebt(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllResignationProceduresEmployeeDebt = [];
                            if (response.status === 200) {
                                $scope.GetAllResignationProceduresEmployeeDebt = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            // Close
            $scope.Close = function () {
                $scope.IsSave = false;
                $("#KenWindownResignationUpdate").closest(".k-window-content").data("kendoWindow").close();
            };

            // Load datasource for ReviewerId autocomplete box
            $scope.GetEmployeeForReviewer = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.model.ReviewerAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item ReviewerId autocomplete box
            $scope.onSelectReviewerId = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForReviewer.data()[index];
                $scope.model.ReviewerId = itemSelected.Id;
                $scope.model.ReviewerAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#ReviewerId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item ReviewerId autocomplete box
            $scope.onChangeReviewerId = function () {
                var matchCode = false;
                $scope.GetEmployeeForReviewer.data().forEach(function (item) {
                    if ($scope.model.ReviewerId === item.Id) {
                        $scope.model.ReviewerAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.model.ReviewerId = STRING_EMPTY;
                    $scope.model.ReviewerAutoComplete = STRING_EMPTY;
                }
            };

            // onChangeReviewerIdValue
            $scope.onChangeReviewerIdValue = function () {
                if ($scope.model.ReviewerAutoComplete === STRING_EMPTY) {
                    $scope.model.ReviewerId = STRING_EMPTY;
                    $scope.model.ReviewerAutoComplete = STRING_EMPTY;
                }
            };

        }]);

})(window.angular);
