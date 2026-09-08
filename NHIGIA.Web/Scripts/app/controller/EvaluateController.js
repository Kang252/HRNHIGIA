(function (angular) {
    "use strict";
    hrmApp.controller('EvaluateController', [
        '$scope', 'EvaluateService', 'ListCategoryService', 'EmployeeInformationService', '$window', '$http',
        function ($scope, EvaluateService, ListCategoryService, EmployeeInformationService, $window, $http) {

            $(document).ready(function () {
                $('.evaluate_active').removeClass('active');
                $('.evaluate_active').addClass('active');
            });

            // Define
            $scope.IsSave = false;
            $scope.IsSaveEvaluateDetail = false;
            $scope.formErrors = {};
            $scope.CheckDuplicate = false;
            $scope.CheckDuplicateEvaluateDetail = false;
            $scope.CheckValidate = false;

            $scope.IdTemp = 0;
            $scope.IdTempEdit = 0;

            $scope.EvaluateDetailDataSource = {};
            $scope.IdTempEvaluateDetail = 0;
            $scope.IdTempEvaluateDetailEdit = 0;
            $scope.ResultTemp = 0;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.CheckToDate = false;
            $scope.MessageCheckToDate = "Đến ngày phải lớn hơn hoặc bằng Từ ngày";
            $scope.CheckEvaluationTerm = false;
            $scope.MessageCheckEvaluationTerm = "Thời hạn đánh giá phải thuộc khoảng thời gian Từ ngày tới Đến ngày của đợt đánh giá";

            function onHidden() {
                $scope.ShowList = true;
                $scope.ShowInfo = false;
            }
            onHidden();

            function onShow() {
                $scope.ShowList = false;
                $scope.ShowInfo = true;
            }

            // onChange
            function onChange(arg) {
                $scope.IdTemp = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.IdTempEdit = arg.sender.dataItem(arg.sender.select()).Id;
                $("#ButtonEdit").prop("disabled", false);
                $("#ButtonDelete").prop("disabled", false);
                $("#ButtonEvaluate").prop("disabled", false);
            }

            // onDisable
            function onDisable() {
                $("#ButtonEdit").prop("disabled", true);
                $("#ButtonDelete").prop("disabled", true);
                $("#ButtonEvaluate").prop("disabled", true);
                $scope.IdTemp = 0;
            }

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorEmployeeName = false;
            }

            // Designer gird
            $scope.Section = {
                dataSource: EvaluateService.GetAllEvaluate(),
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
                        field: "NameOfAudit", title: "Tên đợt đánh giá", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EvaluationPeriodName", title: "Kỳ đánh giá", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "Since", title: "Ngày bắt đầu", width: "150px",
                        template: "<span>#= (Since == null) ? '' : kendo.toString(kendo.parseDate(Since, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "ToDate", title: "Ngày kết thúc", width: "150px",
                        template: "<span>#= (ToDate == null) ? '' : kendo.toString(kendo.parseDate(ToDate, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EvaluationTerm", title: "Hạn đánh giá", width: "150px",
                        template: "<span>#= (EvaluationTerm == null) ? '' : kendo.toString(kendo.parseDate(EvaluationTerm, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EvaluationStatusName", title: "Trạng thái", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                ]
            };

            // Upsite
            $scope.Upsite = function () {
                $window.scrollTo(0, 0);
            };

            // Close
            $scope.Close = function () {
                $scope.IsSave = false;
                $scope.IdTemp = $scope.IdTempEdit;
                $("#KenWindown").closest(".k-window-content").data("kendoWindow").close();
            };

            // ShowPopup
            $scope.ShowPopup = function (e) {
                $scope.IsSave = false;
                $scope.IsSaveEvaluateDetail = false;
                $scope.CheckDuplicate = false;
                $scope.CheckDuplicateEvaluateDetail = false;
                $scope.CheckToDate = false;
                $scope.CheckEvaluationTerm = false;
                var valueClick = e;
                $("#btnSave").prop("disabled", false);
                $("#btnClose").prop("disabled", false);
                $("#btnSaveEvaluateDetail").prop("disabled", false);
                $("#btnCloseEvaluateDetail").prop("disabled", false);
                $("#btnSaveEvaluateForm").prop("disabled", false);
                $("#btnCloseEvaluateForm").prop("disabled", false);

                $scope.model = {
                    Id: 0,
                    NameOfAudit: STRING_EMPTY,
                    EvaluationPeriodId: STRING_EMPTY,
                    WorkUnitId: STRING_EMPTY,
                    EvaluationStatusId: STRING_EMPTY,
                    PersonInChargeId: STRING_EMPTY,
                    Since: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    ToDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    EvaluationTerm: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear(),
                    BriefDescription: STRING_EMPTY,
                    PersonInChargeAutoComplete: STRING_EMPTY,
                    PersonInChargeNameTemp: STRING_EMPTY
                };

                var windowEvaluate = $("#KenWindown").kendoWindow({
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

                $scope.modelEvaluateDetail = {
                    Id: 0,
                    EvaluateId: STRING_EMPTY,
                    EmployeeId: STRING_EMPTY,
                    EmployeeCode: STRING_EMPTY,
                    EmployeeName: STRING_EMPTY,
                    WorkUnitName: STRING_EMPTY,
                    JobPositionName: STRING_EMPTY
                };

                var windowEvaluateDetail = $("#KenWindownEvaluateDetail").kendoWindow({
                    actions: ["CloseEvaluateDetail"],
                    draggable: true,
                    modal: true,
                    pinned: false,
                    position: {
                        top: 15
                    },
                    resizable: false,
                    width: "55%"
                }).data('kendoWindow');

                $scope.modelEvaluateForm = {
                    Id: 0,
                    EvaluateForm1: 1,
                    EvaluateForm2: 1,
                    EvaluateForm3: 1,
                    EvaluateForm4: 1,
                    EvaluateForm5: 1,
                    EvaluateForm6: 1,
                    EvaluateForm7: 1,
                    EvaluateForm8: 1,
                    EvaluateForm9: 1,
                    EvaluateForm10: 1,
                    EvaluateForm11: 1,
                    EvaluateForm12: 1,
                    EvaluateForm13: 1,
                    EvaluateForm14: 1,
                    EvaluateForm15: 1,
                    EvaluateForm16: 1,
                    EvaluateForm17: 1,
                    EvaluateForm18: 1,
                    EvaluateForm19: 1,
                    EvaluateForm20: 1
                };

                var windowKenWindownEvaluateForm = $("#KenWindownEvaluateForm").kendoWindow({
                    actions: ["CloseEvaluateForm"],
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
                    case "ADD":
                        onHidden();
                        windowEvaluate.title("Thêm mới");
                        $scope.Valid.$setUntouched();
                        windowEvaluate.open();
                        windowEvaluate.center();
                        break;
                    case "EDIT":
                        EvaluateService.GetEvaluateById($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.Success === true) {
                                $scope.model.Id = $scope.IdTemp;
                                $scope.model.NameOfAudit = response.data.Data.NameOfAudit;
                                $scope.model.EvaluationPeriodId = response.data.Data.EvaluationPeriodId;
                                $scope.model.WorkUnitId = response.data.Data.WorkUnitId;
                                $scope.model.EvaluationStatusId = response.data.Data.EvaluationStatusId;
                                $scope.model.PersonInChargeId = response.data.Data.PersonInChargeId;
                                $scope.model.Since = kendo.parseDate(response.data.Data.Since, DATE_FORMAT);
                                $scope.model.ToDate = kendo.parseDate(response.data.Data.ToDate, DATE_FORMAT);
                                $scope.model.EvaluationTerm = kendo.parseDate(response.data.Data.EvaluationTerm, DATE_FORMAT);
                                $scope.model.BriefDescription = response.data.Data.BriefDescription;
                                $scope.model.PersonInChargeAutoComplete = response.data.Data.PersonInChargeName;
                                $scope.model.PersonInChargeNameTemp = response.data.Data.PersonInChargeName;
                                windowEvaluate.title("Sửa dữ liệu");
                                windowEvaluate.open();
                                windowEvaluate.center();
                                $scope.IdTemp = 0;
                                onHidden();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                $scope.IdTemp = 0;
                                onHidden();
                            }
                        });
                        break;
                    case "DELETE":
                        onHidden();
                        if ($scope.IdTemp > 0) {
                            bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                                if (result === true) {
                                    loading();
                                    var data = angular.copy($scope.model);
                                    data.Id = $scope.IdTemp;
                                    EvaluateService.DeleteEvaluate(data).then(function success(response) {
                                        if (response.data.status === 200) {
                                            bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                            RefreshKendoGrid("Table");
                                            stopLoading();
                                            $scope.IdTemp = 0;
                                            onHidden();
                                        } else {
                                            bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                            stopLoading();
                                            $scope.IdTemp = 0;
                                            onHidden();
                                        }
                                    });
                                }
                            });
                        } else {
                            loading();
                            var data = angular.copy($scope.model);
                            EvaluateService.DeleteEvaluate(data).then(function success(response) {
                                if (response.data.status === 404) {
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    stopLoading();
                                    onHidden();
                                }
                            });
                        }
                        break;
                    case "REFRESH":
                        RefreshKendoGrid("Table");
                        break;
                    case "BACK":
                        $window.location.reload();
                        break;
                    case "EXPORT_EXCEL":
                        $("#exportExcelForm").remove();

                        var form = document.createElement("form");
                        form.setAttribute("method", "post");
                        form.setAttribute("action", EvaluateExportForExcelUrl);
                        form.setAttribute("id", "exportExcelForm");
                        form.setAttribute("target", "_blank");

                        var hiddenField = document.createElement("input");
                        hiddenField.setAttribute("name", "jsonRequest");
                        hiddenField.setAttribute("value", "jsonRequest");
                        hiddenField.setAttribute("type", "hidden");

                        form.appendChild(hiddenField);
                        document.body.appendChild(form);

                        $("#exportExcelForm").submit();
                        break;
                    case "EVALUATE":
                        onShow();
                        if ($scope.IdTemp > 0) {
                            $scope.LoadDataEvaluateDetail($scope.IdTemp);
                        } else {
                            loading();
                            bootbox.alert("<span style='color:red; text-align:justify;'>" + CANNOT_FIND_ANY_WITH_GIVEN_ID + "</span>");
                            stopLoading();
                        }
                        break;
                    case "LOAD":
                        if ($scope.IdTemp > 0) {
                            $scope.LoadDataEvaluateDetail($scope.IdTemp);
                        }
                        break;
                    case "ADD_ASSESSED_AUDIENCE":
                        onShow();
                        $scope.CheckValidate = true;
                        windowEvaluateDetail.title("Thêm đối tượng");
                        windowEvaluateDetail.open();
                        windowEvaluateDetail.center();
                        break;
                    case "REMOVE":
                        onShow();
                        if ($scope.IdTempEvaluateDetail > 0) {
                            bootbox.confirm(MSG_DELETED_REMOVE, function (result) {
                                if (result === true) {
                                    loading();
                                    var data = angular.copy($scope.modelEvaluateDetail);
                                    data.Id = $scope.IdTempEvaluateDetail;
                                    EvaluateService.DeleteEvaluateDetail(data).then(function success(response) {
                                        if (response.data.status === 200) {
                                            bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                            $scope.LoadDataEvaluateDetail($scope.IdTemp);
                                            stopLoading();
                                            $scope.IdTempEvaluateDetail = 0;
                                            onShow();
                                        } else {
                                            bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                            stopLoading();
                                            $scope.IdTempEvaluateDetail = 0;
                                            onShow();
                                        }
                                    });
                                }
                            });
                        } else {
                            loading();
                            var data = angular.copy($scope.modelEvaluateDetail);
                            EvaluateService.DeleteEvaluateDetail(data).then(function success(response) {
                                if (response.data.status === 404) {
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    stopLoading();
                                    onShow();
                                }
                            });
                        }
                        break;
                    case "EVALUATE_FORM":
                        if ($scope.ResultTemp != null) {
                            bootbox.alert("<span style='color:green; text-align:justify;'>Đối tượng này đã được đánh giá, vui lòng chọn đối tượng khác để đánh giá</span>");
                        } else {
                            onShow();
                            windowKenWindownEvaluateForm.title("Phiếu đánh giá");
                            windowKenWindownEvaluateForm.open();
                            windowKenWindownEvaluateForm.center();
                        }
                        break;
                    default:
                        break;
                }
            };

            // BindDataForDropdown
            function BindDataForDropdown() {
                ListCategoryService.GetDataForDropdown(WorkUnit).then(function (response) {
                    $scope.WorkUnitDropdownlist = [];
                    if (response.status === 200) {
                        $scope.WorkUnitDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(EvaluationPeriod).then(function (response) {
                    $scope.EvaluationPeriodDropdownlist = [];
                    if (response.status === 200) {
                        $scope.EvaluationPeriodDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(EvaluationStatus).then(function (response) {
                    $scope.EvaluationStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.EvaluationStatusDropdownlist = response.data;
                    }
                });
            }
            BindDataForDropdown();

            // Save
            $scope.Save = function (form) {
                $scope.IsSave = true;
                if (form !== null && !form.$valid) {
                    return;
                }

                loadingPopUp();
                var data = angular.copy($scope.model);
                data.Since = kendo.parseDate($scope.model.Since, DATE_FORMAT);
                data.ToDate = kendo.parseDate($scope.model.ToDate, DATE_FORMAT);
                data.EvaluationTerm = kendo.parseDate($scope.model.EvaluationTerm, DATE_FORMAT);

                // CheckToDate
                if (data.Since != null && data.ToDate != null && data.Since.getTime() > data.ToDate.getTime()) {
                    $scope.CheckToDate = true;
                    stopLoadingPopUp();
                    return;
                } else {
                    $scope.CheckToDate = false;
                }
                // CheckEvaluationTerm
                if (data.Since != null && data.ToDate != null && data.EvaluationTerm != null && data.Since.getTime() > data.EvaluationTerm.getTime() && data.ToDate.getTime() > data.EvaluationTerm.getTime()) {
                    $scope.CheckEvaluationTerm = true;
                    stopLoadingPopUp();
                    return;
                } else {
                    $scope.CheckEvaluationTerm = false;
                }

                $("#btnSave").prop("disabled", true);
                $("#btnClose").prop("disabled", true);

                EvaluateService.SaveEvaluate(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindown").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        RefreshKendoGrid("Table");
                        stopLoadingPopUp();
                        $scope.IdTemp = 0;
                        onHidden();
                    } else if (response.data.status === 409) {
                        $scope.CheckDuplicate = true;
                        $("#btnSave").prop("disabled", false);
                        $("#btnClose").prop("disabled", false);
                        $scope.MessageErrorForDuplicate = $scope.model.NameOfAudit + response.data.message;
                        stopLoadingPopUp();
                        onHidden();
                    } else {
                        $("#KenWindown").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                        $scope.IdTemp = 0;
                        onHidden();
                    }
                });
            };

            // Load datasource for PersonInChargeId autocomplete box
            $scope.GetEmployeeForPersonInCharge = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.model.PersonInChargeAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item PersonInChargeId autocomplete box
            $scope.onSelectPersonInCharge = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForPersonInCharge.data()[index];
                $scope.model.PersonInChargeId = itemSelected.Id;
                $scope.model.PersonInChargeAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#PersonInChargeId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item PersonInChargeId autocomplete box
            $scope.onChangePersonInCharge = function () {
                if ($scope.model.PersonInChargeAutoComplete === $scope.model.PersonInChargeNameTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForPersonInCharge.data().forEach(function (item) {
                    if ($scope.model.PersonInChargeId === item.Id) {
                        $scope.model.PersonInChargeAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.model.PersonInChargeId = STRING_EMPTY;
                    $scope.model.PersonInChargeAutoComplete = STRING_EMPTY;
                }
            };

            // onChangePersonInChargeValue
            $scope.onChangePersonInChargeValue = function () {
                if ($scope.model.PersonInChargeAutoComplete === STRING_EMPTY) {
                    $scope.model.PersonInChargeId = STRING_EMPTY;
                    $scope.model.PersonInChargeAutoComplete = STRING_EMPTY;
                }
            };

            $scope.LoadDataEvaluateDetail = function (evaluateId) {
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllEvaluateDetailUrl,
                                cache: false,
                                data: JSON.stringify({
                                    evaluateId: evaluateId,
                                    page: params.page,
                                    pageSize: params.pageSize,
                                    sortColumn: params.sort.split('-')[0],
                                    sortType: params.sort.split('-')[1],
                                    filterColumn: params.filter
                                }),
                                contentType: "application/json"
                            }).success(function (data) {
                                options.success(data);
                                stopLoading();
                            }).error(function (data) {
                                options.error();
                            });
                        }
                    },
                    batch: false,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    pageSize: 10,
                    schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
                });
                $scope.EvaluateDetailDataSource = response;
                $('#TableEvaluateDetail').data('kendoGrid').setDataSource($scope.EvaluateDetailDataSource);
                $('#TableEvaluateDetail').data('kendoGrid').refresh();
            };

            // onChangeEvaluateDetail
            function onChangeEvaluateDetail(arg) {
                $scope.IdTempEvaluateDetail = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.IdTempEvaluateDetailEdit = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.ResultTemp = arg.sender.dataItem(arg.sender.select()).Result;
                $("#ButtonRemove").prop("disabled", false);
                $("#ButtonEvaluationForm").prop("disabled", false);
            }

            // onDisableEvaluateDetail
            function onDisableEvaluateDetail() {
                $("#ButtonRemove").prop("disabled", true);
                $("#ButtonEvaluationForm").prop("disabled", true);
                $scope.IdTempEvaluateDetail = 0;
            }

            // SectionEvaluateDetail
            $scope.SectionEvaluateDetail = {
                dataSource: $scope.EvaluateDetailDataSource,
                resizeable: true,
                autoBind: true,
                sortable: {
                    mode: SINGLE,
                    allowUnsort: true
                },
                change: onChangeEvaluateDetail,
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
                    onDisableEvaluateDetail();
                },
                sort: function (e) {
                    onDisableEvaluateDetail();
                },
                dataBinding: function (e) {
                    onDisableEvaluateDetail();
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
                        onDisableEvaluateDetail();
                    },
                },
                columns: [
                    {
                        field: "EvaluateStatus", title: "Trạng thái đánh giá", width: "110px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EmployeeCode", title: "Mã nhân viên", width: "100px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EmployeeName", title: "Họ và tên", width: "100px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "WorkUnitName", title: "Đơn vị công tác", width: "100px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "JobPositionName", title: "Vị trí công việc", width: "100px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "Result", title: "Kết quả", width: "100px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "Rank", title: "Xếp hạng", width: "100px",
                        filterable: false
                    },
                ]
            };

            // CloseEvaluateDetail
            $scope.CloseEvaluateDetail = function () {
                $scope.IsSaveEvaluateDetail = false;
                $scope.IdTempEvaluateDetail = $scope.IdTempEvaluateDetailEdit;
                onShowMessageValidate();
                $("#KenWindownEvaluateDetail").closest(".k-window-content").data("kendoWindow").close();
            };

            // Load datasource for EmployeeName autocomplete box
            $scope.GetEmployee = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelEvaluateDetail.EmployeeName)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item EmployeeName autocomplete box
            $scope.onSelectEmployee = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployee.data()[index];
                $scope.modelEvaluateDetail.EmployeeId = itemSelected.Id;
                $scope.modelEvaluateDetail.EmployeeName = itemSelected.EmployeeName;
                $scope.modelEvaluateDetail.EmployeeCode = itemSelected.EmployeeCode;
                $scope.modelEvaluateDetail.WorkUnitName = itemSelected.WorkUnitName;
                $scope.modelEvaluateDetail.JobPositionName = itemSelected.JobPositionName;
                $scope.$applyAsync();
                $("#EmployeeId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item EmployeeName autocomplete box
            $scope.onChangeEmployee = function () {
                if ($scope.modelEvaluateDetail.EmployeeName === $scope.model.DirectManagementNameTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployee.data().forEach(function (item) {
                    if ($scope.modelEvaluateDetail.EmployeeId === item.Id) {
                        $scope.modelEvaluateDetail.EmployeeName = item.EmployeeName;
                        $scope.modelEvaluateDetail.EmployeeCode = item.EmployeeCode;
                        $scope.modelEvaluateDetail.WorkUnitName = item.WorkUnitName;
                        $scope.modelEvaluateDetail.JobPositionName = item.JobPositionName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelEvaluateDetail.EmployeeId = STRING_EMPTY;
                    $scope.modelEvaluateDetail.EmployeeName = STRING_EMPTY;
                    $scope.modelEvaluateDetail.EmployeeCode = STRING_EMPTY;
                    $scope.modelEvaluateDetail.WorkUnitName = STRING_EMPTY;
                    $scope.modelEvaluateDetail.JobPositionName = STRING_EMPTY;
                }
            };

            // onChangeEmployeeValue
            $scope.onChangeEmployeeValue = function () {
                if ($scope.modelEvaluateDetail.EmployeeName === STRING_EMPTY) {
                    $scope.modelEvaluateDetail.EmployeeId = STRING_EMPTY;
                    $scope.modelEvaluateDetail.EmployeeName = STRING_EMPTY;
                    $scope.modelEvaluateDetail.EmployeeCode = STRING_EMPTY;
                    $scope.modelEvaluateDetail.WorkUnitName = STRING_EMPTY;
                    $scope.modelEvaluateDetail.JobPositionName = STRING_EMPTY;
                }
            };

            // onChangeValuePage
            $scope.onChangeValuePage = function (e) {
                switch (e) {
                    case "EmployeeName":
                        if ($scope.modelEvaluateDetail.EmployeeName !== STRING_EMPTY) {
                            $scope.showHasErrorEmployeeName = false;
                        } else {
                            $scope.showHasErrorEmployeeName = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // SaveEvaluateDetail
            $scope.SaveEvaluateDetail = function () {
                $scope.IsSaveEvaluateDetail = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelEvaluateDetail.EmployeeName || $scope.modelEvaluateDetail.EmployeeName === STRING_EMPTY) {
                        $scope.showHasErrorEmployeeName = true;
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var dataEvaluateDetail = angular.copy($scope.modelEvaluateDetail);
                dataEvaluateDetail.EvaluateId = $scope.IdTemp;

                loadingPopUp();
                $("#btnSaveEvaluateDetail").prop("disabled", true);
                $("#btnCloseEvaluateDetail").prop("disabled", true);
                EvaluateService.SaveEvaluateDetail(dataEvaluateDetail).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownEvaluateDetail").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.LoadDataEvaluateDetail($scope.IdTemp);
                        stopLoadingPopUp();
                        $scope.IdTempEvaluateDetail = 0;
                    } else if (response.data.status === 409) {
                        $scope.CheckDuplicateEvaluateDetail = true;
                        $("#btnSaveEvaluateDetail").prop("disabled", false);
                        $("#btnCloseEvaluateDetail").prop("disabled", false);
                        $scope.MessageErrorForDuplicate = $scope.modelEvaluateDetail.EmployeeName + response.data.message;
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownEvaluateDetail").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                        $scope.IdTempEvaluateDetail = 0;
                    }
                });
            };

            // CloseEvaluateForm
            $scope.CloseEvaluateForm = function () {
                $scope.IsSaveEvaluateDetail = false;
                $scope.IdTempEvaluateDetail = $scope.IdTempEvaluateDetailEdit;
                $("#KenWindownEvaluateForm").closest(".k-window-content").data("kendoWindow").close();
            };

            // SaveEvaluateForm
            $scope.SaveEvaluateForm = function () {
                loadingPopUp();
                $("#btnSaveEvaluateForm").prop("disabled", true);
                $("#btnCloseEvaluateForm").prop("disabled", true);
                var data = angular.copy($scope.modelEvaluateForm);
                data.Id = $scope.IdTempEvaluateDetail;
                EvaluateService.SaveEvaluateForm(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownEvaluateForm").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.LoadDataEvaluateDetail($scope.IdTemp);
                        stopLoadingPopUp();
                        $scope.IdTempEvaluateDetail = 0;
                    } else {
                        $("#KenWindownEvaluateForm").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                        $scope.IdTempEvaluateDetail = 0;
                    }
                });
            };

            // Check valid
            $scope.formHasError = function () {
                var hasErrors = false;
                for (var input in $scope.formErrors) {
                    hasErrors = hasErrors || $scope.formErrors[input];
                }
                hasErrors = hasErrors;
                return hasErrors;
            };

            $scope.showErrorMsg = function (input) {
                if (!input) return false;
                if ($scope.IsSave) input.$touched = $scope.IsSave;

                var hasError = (input.$touched || $scope.IsSave) && (input.$error.required || input.$error.invalid);
                $scope.formErrors[input.$name] = hasError;
                return hasError;
            };

        }]);

})(window.angular);
