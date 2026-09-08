(function (angular) {
    "use strict";
    hrmApp.controller('EmployeesOnBusinessTripController', [
        '$scope', 'EmployeesOnBusinessTripService', 'EmployeeInformationService', 'ListCategoryService', '$window',
        'EmployeesOnBusinessTripAdvancesService', 'EmployeesOnBusinessTripPaymentsService', 'EmployeesOnBusinessTripRevenueEstimatesService',
        'AttachmentInformationService', 'EmployeesOnBusinessTripAssignedStaffService',
        function ($scope, EmployeesOnBusinessTripService, EmployeeInformationService, ListCategoryService, $window,
            EmployeesOnBusinessTripAdvancesService, EmployeesOnBusinessTripPaymentsService, EmployeesOnBusinessTripRevenueEstimatesService,
            AttachmentInformationService, EmployeesOnBusinessTripAssignedStaffService) {

            $(document).ready(function () {
                $('.procedure_active').removeClass('active');
                $('.procedure_active').addClass('active');
                $('.procedure_toggle').removeClass('toggled');
                $('.procedure_toggle').addClass('toggled');
                $('.procedure_bussiness_display').removeClass('display_block');
                $('.procedure_bussiness_display').addClass('display_block');
                $('.bussiness_active').removeClass('active');
                $('.bussiness_active').addClass('active');
                $('.procedure_bussiness_toggle').removeClass('toggled');
                $('.procedure_bussiness_toggle').addClass('toggled');
            });

            // Define
            $scope.IsSave = false;
            $scope.ShowList = true;
            $scope.ShowInfo = false;
            $scope.CheckValidate = false;
            $scope.CheckReturnDate = false;

            $scope.modelInformationRecommendedForBusinessTravel = {};

            $scope.IdTemp = 0;
            $scope.IdTempEdit = 0;
            $scope.EmployeesOnBusinessTripIdTemp = 0;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;
            $scope.MessageCheckReturnDate = "Ngày đi phải nhỏ hơn hoặc bằng ngày Về";

            // BindDataToDropdownlist
            function BindDataToDropdownlist() {
                ListCategoryService.GetStatusForDropdown(BrowsingStatus).then(function (response) {
                    $scope.BrowsingStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.BrowsingStatusDropdownlist = response.data;
                    }
                });
            };
            BindDataToDropdownlist();

            // onChange
            function onChange(arg) {
                $scope.IdTemp = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.IdTempEdit = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.EmployeesOnBusinessTripIdTemp = arg.sender.dataItem(arg.sender.select()).Id;
                $("#ButtonEdit").prop("disabled", false);
                $("#ButtonDelete").prop("disabled", false);
            }

            // onDisable
            function onDisable() {
                $("#ButtonEdit").prop("disabled", true);
                $("#ButtonDelete").prop("disabled", true);
                $scope.IdTemp = 0;
            }

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorEmployeeId = false;
                $scope.showHasErrorDayTo = false;
                $scope.showHasErrorReturnDate = false;
                $scope.showHasErrorDeadline = false;
                $scope.showHasErrorBrowsingStatusId = false;
            }

            // Designer gird
            $scope.Section = {
                dataSource: EmployeesOnBusinessTripService.GetAllEmployeesOnBusinessTrip(),
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
                        field: "BrowsingStatusName", title: "Trạng thái", width: "150px", locked: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EmployeeCode", title: "Mã nhân viên", width: "150px", locked: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EmployeeName", title: "Họ và tên", width: "150px", locked: true,
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
                        field: "RecommendedDate", title: "Ngày đề nghị công tác", width: "180px",
                        template: "<span>#= (RecommendedDate === null) ? '' : kendo.toString(kendo.parseDate(RecommendedDate, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "DayTo", title: "Ngày đi", width: "150px",
                        template: "<span>#= (DayTo === null) ? '' : kendo.toString(kendo.parseDate(DayTo, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "ReturnDate", title: "Ngày về", width: "150px",
                        template: "<span>#= (ReturnDate === null) ? '' : kendo.toString(kendo.parseDate(ReturnDate, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "WorkingPlace", title: "Địa điểm công tác", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "AmountProposedForAdvance", title: "Số tiền đề nghị tạm ứng", width: "180px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "Deadline", title: "Thời hạn hoàn thành", width: "150px",
                        template: "<span>#= (Deadline === null) ? '' : kendo.toString(kendo.parseDate(Deadline, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "EmployeeApprovedName", title: "Người duyệt", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    }
                ]
            };

            // ShowPopup
            $scope.ShowPopup = function (e) {
                $scope.IsSave = false;
                $scope.CheckValidate = true;
                onShowMessageValidate();
                var valueClick = e;

                $scope.modelInformationRecommendedForBusinessTravel = {
                    Id: 0,
                    EmployeeId: STRING_EMPTY,
                    EmployeeApprovedId: STRING_EMPTY,
                    DayTo: STRING_EMPTY,
                    ReturnDate: STRING_EMPTY,
                    WorkingPlace: STRING_EMPTY,
                    WorkingPurpose: STRING_EMPTY,
                    RecommendedDate: STRING_EMPTY,
                    Deadline: STRING_EMPTY,
                    AmountProposedForAdvance: 0,
                    AmountOfAdvance: 0,
                    ReasonForAdvance: STRING_EMPTY,
                    RequireToBeSupported: STRING_EMPTY,
                    BrowsingStatusId: 4, // Chờ duyệt
                    ReasonsForNotBrowsing: STRING_EMPTY,
                    EmployeeAutoComplete: STRING_EMPTY,
                    EmployeeCode: STRING_EMPTY,
                    JobPositionName: STRING_EMPTY,
                    WorkUnitName: STRING_EMPTY,
                    EmployeeApprovedAutoComplete: STRING_EMPTY,
                    RequireToBeSupported_Car1: STRING_EMPTY,
                    RequireToBeSupported_CarId: STRING_EMPTY,
                    RequireToBeSupported_Car2: STRING_EMPTY,
                    RequireToBeSupported_Car3: STRING_EMPTY,
                    RequireToBeSupported_Plane1: STRING_EMPTY,
                    RequireToBeSupported_PlaneId: STRING_EMPTY,
                    RequireToBeSupported_Plane2: STRING_EMPTY,
                    RequireToBeSupported_Plane3: STRING_EMPTY,
                    RequireToBeSupported_Hotel1: STRING_EMPTY,
                    RequireToBeSupported_HotelId: STRING_EMPTY,
                    RequireToBeSupported_Hotel2: STRING_EMPTY,
                    RequireToBeSupported_Hotel3: STRING_EMPTY,
                    RequireToBeSupported_Other1: STRING_EMPTY,
                    RequireToBeSupported_OtherId: STRING_EMPTY,
                    RequireToBeSupported_Other2: STRING_EMPTY,
                    RequireToBeSupported_Other3: STRING_EMPTY,
                    EmployeeAutoCompleteTemp: STRING_EMPTY,
                    EmployeeApprovedAutoCompleteTemp: STRING_EMPTY,
                    RequireToBeSupported_Car1Temp: STRING_EMPTY,
                    RequireToBeSupported_Plane1Temp: STRING_EMPTY,
                    RequireToBeSupported_Hotel1Temp: STRING_EMPTY,
                    RequireToBeSupported_Other1Temp: STRING_EMPTY
                };

                switch (valueClick) {
                    case "ADD":
                        loadingProfilePage();
                        $scope.ShowList = false;
                        $scope.ShowInfo = true;
                        stopLoadingProfilePage();
                        break;
                    case "EDIT":
                        loadingProfilePage();
                        if ($scope.IdTemp === 0) {
                            EmployeesOnBusinessTripService.GetEmployeesOnBusinessTripById($scope.IdTemp).then(function (response) {
                                if (response.data.status === 404) {
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    $scope.IdTemp = 0;
                                    stopLoadingProfilePage();
                                    return;
                                }
                            });
                        } else {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                            EmployeesOnBusinessTripService.GetEmployeesOnBusinessTripById($scope.IdTemp).then(function (response) {
                                if (response.status === 200 && response.data.Success === true) {
                                    $scope.modelInformationRecommendedForBusinessTravel.Id = $scope.IdTemp;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeId = response.data.Data.EmployeeId;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete = response.data.Data.EmployeeName;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoCompleteTemp = response.data.Data.EmployeeName;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeCode = response.data.Data.EmployeeCode;
                                    $scope.modelInformationRecommendedForBusinessTravel.JobPositionName = response.data.Data.JobPositionName;
                                    $scope.modelInformationRecommendedForBusinessTravel.WorkUnitName = response.data.Data.WorkUnitName;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedId = response.data.Data.EmployeeApprovedId;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete = response.data.Data.EmployeeApprovedName;
                                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoCompleteTemp = response.data.Data.EmployeeApprovedName;
                                    $scope.modelInformationRecommendedForBusinessTravel.DayTo = kendo.parseDate(response.data.Data.DayTo, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.ReturnDate = kendo.parseDate(response.data.Data.ReturnDate, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.WorkingPlace = response.data.Data.WorkingPlace;
                                    $scope.modelInformationRecommendedForBusinessTravel.WorkingPurpose = response.data.Data.WorkingPurpose;
                                    $scope.modelInformationRecommendedForBusinessTravel.RecommendedDate = kendo.parseDate(response.data.Data.RecommendedDate, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.Deadline = kendo.parseDate(response.data.Data.Deadline, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.AmountProposedForAdvance = response.data.Data.AmountProposedForAdvance;
                                    $scope.modelInformationRecommendedForBusinessTravel.AmountOfAdvance = response.data.Data.AmountOfAdvance;
                                    $scope.modelInformationRecommendedForBusinessTravel.ReasonForAdvance = response.data.Data.ReasonForAdvance;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported = response.data.Data.RequireToBeSupported;
                                    $scope.modelInformationRecommendedForBusinessTravel.BrowsingStatusId = response.data.Data.BrowsingStatusId;
                                    $scope.modelInformationRecommendedForBusinessTravel.ReasonsForNotBrowsing = response.data.Data.ReasonsForNotBrowsing;

                                    // RequireToBeSupported to model
                                    if (response.data.Data.RequireToBeSupported && response.data.Data.RequireToBeSupported !== null) {
                                        var obj = JSON.parse(response.data.Data.RequireToBeSupported);
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 = obj.RequireToBeSupported_Car1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId = obj.RequireToBeSupported_CarId;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2 = kendo.parseDate(obj.RequireToBeSupported_Car2, DATE_FORMAT);
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car3 = obj.RequireToBeSupported_Car3;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 = obj.RequireToBeSupported_Plane1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId = obj.RequireToBeSupported_PlaneId;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2 = kendo.parseDate(obj.RequireToBeSupported_Plane2, DATE_FORMAT);
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane3 = obj.RequireToBeSupported_Plane3;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 = obj.RequireToBeSupported_Hotel1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId = obj.RequireToBeSupported_HotelId;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2 = kendo.parseDate(obj.RequireToBeSupported_Hotel2, DATE_FORMAT);
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel3 = obj.RequireToBeSupported_Hotel3;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 = obj.RequireToBeSupported_Other1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId = obj.RequireToBeSupported_OtherId;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2 = kendo.parseDate(obj.RequireToBeSupported_Other2, DATE_FORMAT);
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other3 = obj.RequireToBeSupported_Other3;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1Temp = obj.RequireToBeSupported_Car1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1Temp = obj.RequireToBeSupported_Plane1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1Temp = obj.RequireToBeSupported_Hotel1;
                                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1Temp = obj.RequireToBeSupported_Other1;
                                    }

                                    stopLoadingProfilePage();
                                }
                            });
                        }
                        break;
                    case "DELETE":
                        if ($scope.IdTemp > 0) {
                            bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                                if (result === true) {
                                    loading();
                                    var data = angular.copy($scope.modelInformationRecommendedForBusinessTravel);
                                    data.Id = $scope.IdTemp;
                                    EmployeesOnBusinessTripService.DeleteEmployeesOnBusinessTrip(data).then(function success(response) {
                                        if (response.data.status === 200) {
                                            bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                            RefreshKendoGrid("Table");
                                            stopLoading();
                                            $scope.IdTemp = 0;
                                        } else {
                                            bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                            stopLoading();
                                            $scope.IdTemp = 0;
                                        }
                                    });
                                }
                            });
                        } else {
                            loading();
                            var data = angular.copy($scope.modelInformationRecommendedForBusinessTravel);
                            EmployeesOnBusinessTripService.DeleteEmployeesOnBusinessTrip(data).then(function success(response) {
                                if (response.data.status === 404) {
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    stopLoading();
                                }
                            });
                        }
                        break;
                    case "CANCEL":
                        loadingProfilePage();
                        $window.location.reload();
                        stopLoadingProfilePage();
                        break;
                    case "EXPORT_EXCEL":
                        // to do
                        break;
                    case "REFRESH":
                        loadingProfilePage();
                        RefreshKendoGrid("Table");
                        stopLoadingProfilePage();
                        break;
                    default:
                        break;
                }
            };

            // Save
            $scope.Save = function () {
                $scope.IsSave = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete || $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete === STRING_EMPTY) {
                        $scope.showHasErrorEmployeeId = true;
                    }
                    if (!$scope.modelInformationRecommendedForBusinessTravel.DayTo || $scope.modelInformationRecommendedForBusinessTravel.DayTo === STRING_EMPTY) {
                        $scope.showHasErrorDayTo = true;
                    }
                    if (!$scope.modelInformationRecommendedForBusinessTravel.ReturnDate || $scope.modelInformationRecommendedForBusinessTravel.ReturnDate === STRING_EMPTY) {
                        $scope.showHasErrorReturnDate = true;
                    }
                    if (!$scope.modelInformationRecommendedForBusinessTravel.Deadline || $scope.modelInformationRecommendedForBusinessTravel.Deadline === STRING_EMPTY) {
                        $scope.showHasErrorDeadline = true;
                    }
                    if (!$scope.modelInformationRecommendedForBusinessTravel.BrowsingStatusId || $scope.modelInformationRecommendedForBusinessTravel.BrowsingStatusId === STRING_EMPTY) {
                        $scope.showHasErrorBrowsingStatusId = true;
                    }
                    if ($scope.showHasErrorEmployeeId === true || $scope.showHasErrorDayTo === true || $scope.showHasErrorReturnDate === true || $scope.showHasErrorDeadline === true || $scope.showHasErrorBrowsingStatusId === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var dataInformationRecommendedForBusinessTravel = angular.copy($scope.modelInformationRecommendedForBusinessTravel);
                dataInformationRecommendedForBusinessTravel.DayTo = kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.DayTo, DATE_FORMAT);
                dataInformationRecommendedForBusinessTravel.ReturnDate = kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.ReturnDate, DATE_FORMAT);
                dataInformationRecommendedForBusinessTravel.RecommendedDate = kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.RecommendedDate, DATE_FORMAT);
                dataInformationRecommendedForBusinessTravel.Deadline = kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.Deadline, DATE_FORMAT);
                dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2 = $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2 ? kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2, DATE_FORMAT) : STRING_EMPTY;
                dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2 = $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2 ? kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2, DATE_FORMAT) : STRING_EMPTY;
                dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2 = $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2 ? kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2, DATE_FORMAT) : STRING_EMPTY;
                dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2 = $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2 ? kendo.parseDate($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2, DATE_FORMAT) : STRING_EMPTY;

                var requireToBeSupported_Car2_Value = STRING_EMPTY;
                var requireToBeSupported_Plane2_Value = STRING_EMPTY;
                var requireToBeSupported_Hotel2_Value = STRING_EMPTY;
                var requireToBeSupported_Other2_Value = STRING_EMPTY;
                if (dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2 !== STRING_EMPTY) {
                    requireToBeSupported_Car2_Value = "/Date(" + dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2.getTime() + ")/";
                }
                if (dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2 !== STRING_EMPTY) {
                    requireToBeSupported_Plane2_Value = "/Date(" + dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2.getTime() + ")/";
                }
                if (dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2 !== STRING_EMPTY) {
                    requireToBeSupported_Hotel2_Value = "/Date(" + dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2.getTime() + ")/";
                }
                if (dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2 !== STRING_EMPTY) {
                    requireToBeSupported_Other2_Value = "/Date(" + dataInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2.getTime() + ")/";
                }

                var requireToBeSupportedArray = {
                    "RequireToBeSupported_Car1": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1,
                    "RequireToBeSupported_CarId": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId,
                    "RequireToBeSupported_Car2": requireToBeSupported_Car2_Value,
                    "RequireToBeSupported_Car3": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car3,
                    "RequireToBeSupported_Plane1": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1,
                    "RequireToBeSupported_PlaneId": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId,
                    "RequireToBeSupported_Plane2": requireToBeSupported_Plane2_Value,
                    "RequireToBeSupported_Plane3": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane3,
                    "RequireToBeSupported_Hotel1": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1,
                    "RequireToBeSupported_HotelId": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId,
                    "RequireToBeSupported_Hotel2": requireToBeSupported_Hotel2_Value,
                    "RequireToBeSupported_Hotel3": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel3,
                    "RequireToBeSupported_Other1": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1,
                    "RequireToBeSupported_OtherId": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId,
                    "RequireToBeSupported_Other2": requireToBeSupported_Other2_Value,
                    "RequireToBeSupported_Other3": $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other3
                };
                dataInformationRecommendedForBusinessTravel.RequireToBeSupported = JSON.stringify(requireToBeSupportedArray);

                // Check DayTo < ReturnDate
                if (dataInformationRecommendedForBusinessTravel.DayTo !== null && dataInformationRecommendedForBusinessTravel.ReturnDate !== null && dataInformationRecommendedForBusinessTravel.DayTo.getTime() > dataInformationRecommendedForBusinessTravel.ReturnDate.getTime()) {
                    $scope.CheckReturnDate = true;
                    return;
                } else {
                    $scope.CheckReturnDate = false;
                }

                loadingPopUp();
                EmployeesOnBusinessTripService.SaveEmployeesOnBusinessTrip(dataInformationRecommendedForBusinessTravel).then(function success(response) {
                    if (response.data.status === 200) {
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                        if (dataInformationRecommendedForBusinessTravel.Id > 0) {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                        }
                        else {
                            $scope.ShowList = true;
                            $scope.ShowInfo = false;
                            $scope.IdTemp = 0;
                        }
                    } else {
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                        if (dataInformationRecommendedForBusinessTravel.Id > 0) {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                        }
                        else {
                            $scope.ShowList = true;
                            $scope.ShowInfo = false;
                            $scope.IdTemp = 0;
                        }
                    }
                });
            };

            // Load datasource for GetEmployee autocomplete box
            $scope.GetEmployee = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Load datasource for GetEmployeeApproved autocomplete box
            $scope.GetEmployeeApproved = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Load datasource for GetRequireToBeSupported_Car1 autocomplete box
            $scope.GetRequireToBeSupported_Car1 = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Load datasource for GetRequireToBeSupported_Plane1 autocomplete box
            $scope.GetRequireToBeSupported_Plane1 = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Load datasource for GetRequireToBeSupported_Hotel1 autocomplete box
            $scope.GetRequireToBeSupported_Hotel1 = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Load datasource for GetRequireToBeSupported_Other1 autocomplete box
            $scope.GetRequireToBeSupported_Other1 = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item onSelectEmployee autocomplete box
            $scope.onSelectEmployee = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployee.data()[index];
                $scope.modelInformationRecommendedForBusinessTravel.EmployeeId = itemSelected.Id;
                $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete = itemSelected.EmployeeName;
                $scope.modelInformationRecommendedForBusinessTravel.EmployeeCode = itemSelected.EmployeeCode;
                $scope.modelInformationRecommendedForBusinessTravel.JobPositionName = itemSelected.JobPositionName;
                $scope.modelInformationRecommendedForBusinessTravel.WorkUnitName = itemSelected.WorkUnitName;
                $scope.$applyAsync();
                $("#EmployeeId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event onselect item onSelectEmployeeApproved autocomplete box
            $scope.onSelectEmployeeApproved = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeApproved.data()[index];
                $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedId = itemSelected.Id;
                $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#EmployeeApprovedId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event onselect item onSelectRequireToBeSupported_Car1 autocomplete box
            $scope.onSelectRequireToBeSupported_Car1 = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetRequireToBeSupported_Car1.data()[index];
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId = itemSelected.Id;
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#RequireToBeSupported_Car1").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event onselect item onSelectRequireToBeSupported_Plane1 autocomplete box
            $scope.onSelectRequireToBeSupported_Plane1 = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetRequireToBeSupported_Plane1.data()[index];
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId = itemSelected.Id;
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#RequireToBeSupported_Plane1").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event onselect item onSelectRequireToBeSupported_Hotel1 autocomplete box
            $scope.onSelectRequireToBeSupported_Hotel1 = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetRequireToBeSupported_Hotel1.data()[index];
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId = itemSelected.Id;
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#RequireToBeSupported_Hotel1").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event onselect item onSelectRequireToBeSupported_Other1 autocomplete box
            $scope.onSelectRequireToBeSupported_Other1 = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetRequireToBeSupported_Other1.data()[index];
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId = itemSelected.Id;
                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#RequireToBeSupported_Other1").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item onChangeEmployee autocomplete box
            $scope.onChangeEmployee = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete === $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoCompleteTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployee.data().forEach(function (item) {
                    if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeId === item.Id) {
                        $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete = item.EmployeeName;
                        $scope.modelInformationRecommendedForBusinessTravel.EmployeeCode = item.EmployeeCode;
                        $scope.modelInformationRecommendedForBusinessTravel.JobPositionName = item.JobPositionName;
                        $scope.modelInformationRecommendedForBusinessTravel.WorkUnitName = item.WorkUnitName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeCode = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.JobPositionName = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.WorkUnitName = STRING_EMPTY;
                }
            };

            // Event blur item onChangeEmployeeApproved autocomplete box
            $scope.onChangeEmployeeApproved = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete === $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoCompleteTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeApproved.data().forEach(function (item) {
                    if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedId === item.Id) {
                        $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete = STRING_EMPTY;
                }
            };

            // Event blur item onChangeRequireToBeSupported_Car1 autocomplete box
            $scope.onChangeRequireToBeSupported_Car1 = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 === $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1Temp) {
                    return;
                }

                var matchCode = false;
                $scope.GetRequireToBeSupported_Car1.data().forEach(function (item) {
                    if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId === item.Id) {
                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 = STRING_EMPTY;
                }
            };

            // Event blur item onChangeRequireToBeSupported_Plane1 autocomplete box
            $scope.onChangeRequireToBeSupported_Plane1 = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 === $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1Temp) {
                    return;
                }

                var matchCode = false;
                $scope.GetRequireToBeSupported_Plane1.data().forEach(function (item) {
                    if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId === item.Id) {
                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 = STRING_EMPTY;
                }
            };

            // Event blur item onChangeRequireToBeSupported_Hotel1 autocomplete box
            $scope.onChangeRequireToBeSupported_Hotel1 = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 === $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1Temp) {
                    return;
                }

                var matchCode = false;
                $scope.GetRequireToBeSupported_Hotel1.data().forEach(function (item) {
                    if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId === item.Id) {
                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 = STRING_EMPTY;
                }
            };

            // Event blur item onChangeRequireToBeSupported_Other1 autocomplete box
            $scope.onChangeRequireToBeSupported_Other1 = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 === $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1Temp) {
                    return;
                }

                var matchCode = false;
                $scope.GetRequireToBeSupported_Other1.data().forEach(function (item) {
                    if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId === item.Id) {
                        $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 = STRING_EMPTY;
                }
            };

            // onChangeEmployeeValue
            $scope.onChangeEmployeeValue = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete === STRING_EMPTY) {
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeCode = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.JobPositionName = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.WorkUnitName = STRING_EMPTY;
                }
            };

            // onChangeEmployeeApprovedValue
            $scope.onChangeEmployeeApprovedValue = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete === STRING_EMPTY) {
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete = STRING_EMPTY;
                }
            };

            // onChangeRequireToBeSupported_Car1Value
            $scope.onChangeRequireToBeSupported_Car1Value = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 === STRING_EMPTY) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 = STRING_EMPTY;
                }
            };

            // onChangeRequireToBeSupported_Plane1Value
            $scope.onChangeRequireToBeSupported_Plane1Value = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 === STRING_EMPTY) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 = STRING_EMPTY;
                }
            };

            // onChangeRequireToBeSupported_Hotel1Value
            $scope.onChangeRequireToBeSupported_Hotel1Value = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 === STRING_EMPTY) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 = STRING_EMPTY;
                }
            };

            // onChangeRequireToBeSupported_Other1Value
            $scope.onChangeRequireToBeSupported_Other1Value = function () {
                if ($scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 === STRING_EMPTY) {
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId = STRING_EMPTY;
                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 = STRING_EMPTY;
                }
            };

            // onChangeValue
            $scope.onChangeValue = function (e) {
                switch (e) {
                    case "EmployeeId":
                        if ($scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete !== STRING_EMPTY) {
                            $scope.showHasErrorEmployeeId = false;
                        } else {
                            $scope.showHasErrorEmployeeId = true;
                        }
                        break;
                    case "DayTo":
                        if ($scope.modelInformationRecommendedForBusinessTravel.DayTo !== STRING_EMPTY) {
                            $scope.showHasErrorDayTo = false;
                        } else {
                            $scope.showHasErrorDayTo = true;
                        }
                        break;
                    case "ReturnDate":
                        if ($scope.modelInformationRecommendedForBusinessTravel.ReturnDate !== STRING_EMPTY) {
                            $scope.showHasErrorReturnDate = false;
                        } else {
                            $scope.showHasErrorReturnDate = true;
                        }
                        break;
                    case "Deadline":
                        if ($scope.modelInformationRecommendedForBusinessTravel.Deadline !== STRING_EMPTY) {
                            $scope.showHasErrorDeadline = false;
                        } else {
                            $scope.showHasErrorDeadline = true;
                        }
                        break;
                    case "BrowsingStatusId":
                        if ($scope.modelInformationRecommendedForBusinessTravel.BrowsingStatusId !== STRING_EMPTY) {
                            $scope.showHasErrorBrowsingStatusId = false;
                        } else {
                            $scope.showHasErrorBrowsingStatusId = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // Upsite
            $scope.Upsite = function () {
                $window.scrollTo(0, 0);
            };

            // ClickInformation
            $scope.ClickInformation = function (e) {
                switch (e) {
                    case "INFORMATION_RECOMMENDED_FOR_BUSINESS_TRAVEL":
                        loadingProfilePage();
                        $('.save-hide').removeClass('hide');
                        EmployeesOnBusinessTripService.GetEmployeesOnBusinessTripById($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.Success === true) {
                                $scope.modelInformationRecommendedForBusinessTravel.Id = $scope.IdTemp;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeId = response.data.Data.EmployeeId;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoComplete = response.data.Data.EmployeeName;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeAutoCompleteTemp = response.data.Data.EmployeeName;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeCode = response.data.Data.EmployeeCode;
                                $scope.modelInformationRecommendedForBusinessTravel.JobPositionName = response.data.Data.JobPositionName;
                                $scope.modelInformationRecommendedForBusinessTravel.WorkUnitName = response.data.Data.WorkUnitName;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedId = response.data.Data.EmployeeApprovedId;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoComplete = response.data.Data.EmployeeApprovedName;
                                $scope.modelInformationRecommendedForBusinessTravel.EmployeeApprovedAutoCompleteTemp = response.data.Data.EmployeeApprovedName;
                                $scope.modelInformationRecommendedForBusinessTravel.DayTo = kendo.parseDate(response.data.Data.DayTo, DATE_FORMAT);
                                $scope.modelInformationRecommendedForBusinessTravel.ReturnDate = kendo.parseDate(response.data.Data.ReturnDate, DATE_FORMAT);
                                $scope.modelInformationRecommendedForBusinessTravel.WorkingPlace = response.data.Data.WorkingPlace;
                                $scope.modelInformationRecommendedForBusinessTravel.WorkingPurpose = response.data.Data.WorkingPurpose;
                                $scope.modelInformationRecommendedForBusinessTravel.RecommendedDate = kendo.parseDate(response.data.Data.RecommendedDate, DATE_FORMAT);
                                $scope.modelInformationRecommendedForBusinessTravel.Deadline = kendo.parseDate(response.data.Data.Deadline, DATE_FORMAT);
                                $scope.modelInformationRecommendedForBusinessTravel.AmountProposedForAdvance = response.data.Data.AmountProposedForAdvance;
                                $scope.modelInformationRecommendedForBusinessTravel.AmountOfAdvance = response.data.Data.AmountOfAdvance;
                                $scope.modelInformationRecommendedForBusinessTravel.ReasonForAdvance = response.data.Data.ReasonForAdvance;
                                $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported = response.data.Data.RequireToBeSupported;
                                $scope.modelInformationRecommendedForBusinessTravel.BrowsingStatusId = response.data.Data.BrowsingStatusId;
                                $scope.modelInformationRecommendedForBusinessTravel.ReasonsForNotBrowsing = response.data.Data.ReasonsForNotBrowsing;

                                // RequireToBeSupported to model
                                if (response.data.Data.RequireToBeSupported && response.data.Data.RequireToBeSupported !== null) {
                                    var obj = JSON.parse(response.data.Data.RequireToBeSupported);
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1 = obj.RequireToBeSupported_Car1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_CarId = obj.RequireToBeSupported_CarId;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car2 = kendo.parseDate(obj.RequireToBeSupported_Car2, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car3 = obj.RequireToBeSupported_Car3;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1 = obj.RequireToBeSupported_Plane1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_PlaneId = obj.RequireToBeSupported_PlaneId;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane2 = kendo.parseDate(obj.RequireToBeSupported_Plane2, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane3 = obj.RequireToBeSupported_Plane3;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1 = obj.RequireToBeSupported_Hotel1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_HotelId = obj.RequireToBeSupported_HotelId;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel2 = kendo.parseDate(obj.RequireToBeSupported_Hotel2, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel3 = obj.RequireToBeSupported_Hotel3;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1 = obj.RequireToBeSupported_Other1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_OtherId = obj.RequireToBeSupported_OtherId;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other2 = kendo.parseDate(obj.RequireToBeSupported_Other2, DATE_FORMAT);
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other3 = obj.RequireToBeSupported_Other3;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Car1Temp = obj.RequireToBeSupported_Car1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Plane1Temp = obj.RequireToBeSupported_Plane1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Hotel1Temp = obj.RequireToBeSupported_Hotel1;
                                    $scope.modelInformationRecommendedForBusinessTravel.RequireToBeSupported_Other1Temp = obj.RequireToBeSupported_Other1;
                                }

                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "ADVANCES":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        EmployeesOnBusinessTripAdvancesService.GetAllEmployeesOnBusinessTripAdvances(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllEmployeesOnBusinessTripAdvances = [];
                            if (response.status === 200) {
                                $scope.GetAllEmployeesOnBusinessTripAdvances = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "REVENUE_ESTIMATION":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        EmployeesOnBusinessTripRevenueEstimatesService.GetAllEmployeesOnBusinessTripRevenueEstimates(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllEmployeesOnBusinessTripRevenueEstimates = [];
                            if (response.status === 200) {
                                $scope.GetAllEmployeesOnBusinessTripRevenueEstimates = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "PAYMENTS":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        EmployeesOnBusinessTripPaymentsService.GetAllEmployeesOnBusinessTripPayments(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllEmployeesOnBusinessTripPayments = [];
                            if (response.status === 200) {
                                $scope.GetAllEmployeesOnBusinessTripPayments = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "ATTACHMENT":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        AttachmentInformationService.GetAllAttachmentInformation(0, 0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllAttachmentInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllAttachmentInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "EMPLOYEES_ON_BUSINESS_TRIP":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        EmployeesOnBusinessTripAssignedStaffService.GetAllEmployeesOnBusinessTripAssignedStaff($scope.IdTemp).then(function (response) {
                            $scope.GetAllEmployeesOnBusinessTripAssignedStaff = [];
                            if (response.status === 200) {
                                $scope.GetAllEmployeesOnBusinessTripAssignedStaff = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

        }]);

})(window.angular);
