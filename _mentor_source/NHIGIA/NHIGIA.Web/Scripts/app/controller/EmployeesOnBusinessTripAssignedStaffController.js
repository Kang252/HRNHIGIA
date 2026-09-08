(function (angular) {
    "use strict";
    hrmApp.controller('EmployeesOnBusinessTripAssignedStaffController', [
        '$scope', 'EmployeesOnBusinessTripAssignedStaffService', 'EmployeeInformationService',
        function ($scope, EmployeesOnBusinessTripAssignedStaffService, EmployeeInformationService) {

            $scope.IsSaveEmployeesOnBusinessTripAssignedStaff = false;
            $scope.ListIsCheck = [];

            $scope.EmployeesOnBusinessTripIdTemp = $scope.$parent.IdTemp;

            // ShowPopupEmployeesOnBusinessTripAssignedStaff
            $scope.ShowPopupEmployeesOnBusinessTripAssignedStaff = function (e, id) {
                $scope.IsSaveEmployeesOnBusinessTripAssignedStaff = false;
                //$scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveEmployeesOnBusinessTripAssignedStaff").prop("disabled", false);
                $("#btnCloseEmployeesOnBusinessTripAssignedStaff").prop("disabled", false);

                onResetValueCheckBox();

                $scope.modelEmployeesOnBusinessTripAssignedStaff = {
                    Id: 0,
                    ListEmployeeId: []
                };

                var windowEmployeesOnBusinessTripAssignedStaff = $("#KenWindownEmployeesOnBusinessTripAssignedStaff").kendoWindow({
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
                    case "ADD_EMPLOYEESONBUSINESSTRIPASSIGNEDSTAFF":
                        loadingPopUp();
                        windowEmployeesOnBusinessTripAssignedStaff.title("Chọn nhân viên tham gia công tác");
                        RefreshKendoGrid("TableEmployeesOnBusinessTripAssignedStaff");
                        windowEmployeesOnBusinessTripAssignedStaff.open();
                        windowEmployeesOnBusinessTripAssignedStaff.center();
                        stopLoadingPopUp();
                        break;
                    case "DELETE_EMPLOYEESONBUSINESSTRIPASSIGNEDSTAFF":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelEmployeesOnBusinessTripAssignedStaff);
                                data.Id = id;
                                EmployeesOnBusinessTripAssignedStaffService.DeleteEmployeesOnBusinessTripAssignedStaff(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("EMPLOYEES_ON_BUSINESS_TRIP");
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
            $scope.SectionEmployeesOnBusinessTripAssignedStaff = {
                dataSource: EmployeeInformationService.GetEmployee($scope.EmployeesOnBusinessTripIdTemp, "EmployeesOnBusinessTripAssignedStaff"),
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

            // CloseEmployeesOnBusinessTripAssignedStaff
            $scope.CloseEmployeesOnBusinessTripAssignedStaff = function () {
                $("#KenWindownEmployeesOnBusinessTripAssignedStaff").closest(".k-window-content").data("kendoWindow").close();
            };

            //on click of the checkbox:
            $scope.selectRow = function ($event, dataItem) {
                var checked = $event.currentTarget.checked;
                dataItem.IsCheck = checked;

                $scope.ListIsCheck.push({
                    EmployeeId: dataItem.Id,
                    EmployeesOnBusinessTripId: $scope.EmployeesOnBusinessTripIdTemp
                });

                if (dataItem.IsCheck === false) {
                    $scope.ListIsCheck = $scope.ListIsCheck.filter(item => item.EmployeeId !== dataItem.Id);
                }
            };

            //on click of the all checkbox:
            $scope.selectAllRow = function ($event) {
                var checked = $event.currentTarget.checked;
                var grid = $("#TableEmployeesOnBusinessTripAssignedStaff").data("kendoGrid");
                var items = grid.items();
                items.each(function () {
                    var dataItem = grid.dataItem(this);
                    if (dataItem.IsCheck != checked) {
                        dataItem.IsCheck = checked;
                        dataItem.dirty = true;
                        $scope.ListIsCheck.push({
                            EmployeeId: dataItem.Id,
                            EmployeesOnBusinessTripId: $scope.EmployeesOnBusinessTripIdTemp
                        });
                        if (dataItem.IsCheck === false) {
                            $scope.ListIsCheck = [];
                        }
                    }
                });
            };

            // SaveEmployeesOnBusinessTripAssignedStaff
            $scope.SaveEmployeesOnBusinessTripAssignedStaff = function () {
                $scope.IsSaveEmployeesOnBusinessTripAssignedStaff = true;

                if ($scope.ListIsCheck.length === 0) {
                    window.alert("Bạn cần phải chọn ít nhất 1 nhân viên");
                    return;
                }

                var data = angular.copy($scope.modelEmployeesOnBusinessTripAssignedStaff);
                data.ListEmployeeId = $scope.ListIsCheck;

                loadingPopUp();
                $("#btnSaveEmployeesOnBusinessTripAssignedStaff").prop("disabled", true);
                $("#btnCloseEmployeesOnBusinessTripAssignedStaff").prop("disabled", true);
                EmployeesOnBusinessTripAssignedStaffService.SaveEmployeesOnBusinessTripAssignedStaff(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownEmployeesOnBusinessTripAssignedStaff").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("EMPLOYEES_ON_BUSINESS_TRIP");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownEmployeesOnBusinessTripAssignedStaff").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
