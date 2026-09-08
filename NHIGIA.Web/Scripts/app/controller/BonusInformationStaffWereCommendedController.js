(function (angular) {
    "use strict";
    hrmApp.controller('BonusInformationStaffWereCommendedController', [
        '$scope', 'BonusInformationStaffWereCommendedService', 'EmployeeInformationService',
        function ($scope, BonusInformationStaffWereCommendedService, EmployeeInformationService) {

            $scope.IsSaveBonusInformationStaffWereCommended = false;
            $scope.ListIsCheck = [];

            $scope.BonusInformationIdTemp = $scope.$parent.IdTemp;

            $scope.modelBonusInformationStaffWereCommended = {};

            // ShowPopupBonusInformationStaffWereCommended
            $scope.ShowPopupBonusInformationStaffWereCommended = function (e, id) {
                $scope.IsSaveBonusInformationStaffWereCommended = false;
                //$scope.CheckValidate = true;
                var valueClick = e;
                $("#btnSaveBonusInformationStaffWereCommended").prop("disabled", false);
                $("#btnCloseBonusInformationStaffWereCommended").prop("disabled", false);

                onResetValueCheckBox();

                $scope.modelBonusInformationStaffWereCommended = {
                    Id: 0,
                    ListEmployeeId: [],
                    BonusValue: STRING_EMPTY
                };

                var windowBonusInformationStaffWereCommended = $("#KenWindownBonusInformationStaffWereCommended").kendoWindow({
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
                    case "ADD_BONUSINFORMATIONSTAFFWERECOMMENDED":
                        loadingPopUp();
                        windowBonusInformationStaffWereCommended.title("Chọn nhân viên được khen thưởng");
                        RefreshKendoGrid("TableBonusInformationStaffWereCommended");
                        windowBonusInformationStaffWereCommended.open();
                        windowBonusInformationStaffWereCommended.center();
                        stopLoadingPopUp();
                        break;
                    case "DELETE_BONUSINFORMATIONSTAFFWERECOMMENDED":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingPopUp();
                                var data = angular.copy($scope.modelBonusInformationStaffWereCommended);
                                data.Id = id;
                                BonusInformationStaffWereCommendedService.DeleteBonusInformationStaffWereCommended(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        $scope.$parent.ClickInformation("STAFF_WERE_COMMENDED");
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
            $scope.SectionBonusInformationStaffWereCommended = {
                dataSource: EmployeeInformationService.GetEmployee($scope.BonusInformationIdTemp, "BonusInformationStaffWereCommended"),
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

            // CloseBonusInformationStaffWereCommended
            $scope.CloseBonusInformationStaffWereCommended = function () {
                $("#KenWindownBonusInformationStaffWereCommended").closest(".k-window-content").data("kendoWindow").close();
            };

            //on click of the checkbox:
            $scope.selectRow = function ($event, dataItem) {
                var checked = $event.currentTarget.checked;
                dataItem.IsCheck = checked;

                $scope.ListIsCheck.push({
                    EmployeeId: dataItem.Id,
                    BonusInformationId: $scope.BonusInformationIdTemp
                });

                if (dataItem.IsCheck === false) {
                    $scope.ListIsCheck = $scope.ListIsCheck.filter(item => item.EmployeeId !== dataItem.Id);
                }
            };

            //on click of the all checkbox:
            $scope.selectAllRow = function ($event) {
                var checked = $event.currentTarget.checked;
                var grid = $("#TableBonusInformationStaffWereCommended").data("kendoGrid");
                var items = grid.items();
                items.each(function () {
                    var dataItem = grid.dataItem(this);
                    if (dataItem.IsCheck != checked) {
                        dataItem.IsCheck = checked;
                        dataItem.dirty = true;
                        $scope.ListIsCheck.push({
                            EmployeeId: dataItem.Id,
                            BonusInformationId: $scope.BonusInformationIdTemp
                        });
                        if (dataItem.IsCheck === false) {
                            $scope.ListIsCheck = [];
                        }
                    }
                });
            };

            // SaveBonusInformationStaffWereCommended
            $scope.SaveBonusInformationStaffWereCommended = function () {
                $scope.IsSaveBonusInformationStaffWereCommended = true;

                if ($scope.ListIsCheck.length === 0) {
                    window.alert("Bạn cần phải chọn ít nhất 1 nhân viên");
                    return;
                }

                var data = angular.copy($scope.modelBonusInformationStaffWereCommended);
                data.ListEmployeeId = $scope.ListIsCheck;

                loadingPopUp();
                $("#btnSaveBonusInformationStaffWereCommended").prop("disabled", true);
                $("#btnCloseBonusInformationStaffWereCommended").prop("disabled", true);
                BonusInformationStaffWereCommendedService.SaveBonusInformationStaffWereCommended(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownBonusInformationStaffWereCommended").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        $scope.$parent.ClickInformation("STAFF_WERE_COMMENDED");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownBonusInformationStaffWereCommended").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            // SaveChange
            $scope.SaveChange = function (id) {
                loadingPopUp();
                var value = $('#BonusValue' + id).val();
                var chkBox = document.getElementById("Status_" + id);
                var data = angular.copy($scope.modelBonusInformationStaffWereCommended);
                data.Id = id;
                data.BonusValue = value;
                data.Status = chkBox.checked;
                BonusInformationStaffWereCommendedService.SaveOnlyBonusInformationStaffWereCommended(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $scope.$parent.ClickInformation("STAFF_WERE_COMMENDED");
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    } else {
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

        }]);

})(window.angular);
