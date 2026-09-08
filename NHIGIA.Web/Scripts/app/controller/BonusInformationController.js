(function (angular) {
    "use strict";
    hrmApp.controller('BonusInformationController', [
        '$scope', 'BonusInformationService', 'ListCategoryService', '$window', 'EmployeeInformationService', 'BonusInformationStaffWereCommendedService',
        function ($scope, BonusInformationService, ListCategoryService, $window, EmployeeInformationService, BonusInformationStaffWereCommendedService) {

            $(document).ready(function () {
                $('.other_active').removeClass('active');
                $('.other_active').addClass('active');
                $('.other_toggle').removeClass('toggled');
                $('.other_toggle').addClass('toggled');
                $('.other_display').removeClass('display_block');
                $('.other_display').addClass('display_block');
                $('.bonus_active').removeClass('active');
                $('.bonus_active').addClass('active');
                $('.bonus_toggle').removeClass('toggled');
                $('.bonus_toggle').addClass('toggled');
            });

            // Define
            $scope.IsSave = false;
            $scope.ShowList = true;
            $scope.ShowInfo = false;
            $scope.CheckValidate = false;

            $scope.model = {};

            $scope.IdTemp = 0;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            // BindDataToDropdownlist
            function BindDataToDropdownlist() {
                ListCategoryService.GetDataForDropdown(RewardPlan).then(function (response) {
                    $scope.RewardPlanDropdownlist = [];
                    if (response.status === 200) {
                        $scope.RewardPlanDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(RewardReason).then(function (response) {
                    $scope.RewardReasonDropdownlist = [];
                    if (response.status === 200) {
                        $scope.RewardReasonDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(CommendationForm).then(function (response) {
                    $scope.CommendationFormDropdownlist = [];
                    if (response.status === 200) {
                        $scope.CommendationFormDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(BonusBudgetSource).then(function (response) {
                    $scope.BonusBudgetSourceDropdownlist = [];
                    if (response.status === 200) {
                        $scope.BonusBudgetSourceDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(StatusBonus).then(function (response) {
                    $scope.StatusBonusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.StatusBonusDropdownlist = response.data;
                    }
                });
            };
            BindDataToDropdownlist();

            // onChange
            function onChange(arg) {
                $scope.IdTemp = arg.sender.dataItem(arg.sender.select()).Id;

                $("#ButtonEdit").prop("disabled", false);
                $("#ButtonDelete").prop("disabled", false);
            }

            // onDisable
            function onDisable() {
                $("#ButtonEdit").prop("disabled", true);
                $("#ButtonDelete").prop("disabled", true);
                $scope.IdTemp = 0;
            }

            // Designer gird
            $scope.Section = {
                dataSource: BonusInformationService.GetAllBonus(),
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
                        field: "StatusBonusName", title: "Trạng thái", width: "150px", locked: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "BonusDay", title: "Ngày khen thưởng", width: "150px",
                        template: "<span>#= (BonusDay == null) ? '' : kendo.toString(kendo.parseDate(BonusDay, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "DecisionNumber", title: "Số quyết định", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "DecisionDate", title: "Ngày quyết định", width: "150px",
                        template: "<span>#= (DecisionDate == null) ? '' : kendo.toString(kendo.parseDate(DecisionDate, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "RewardReasonName", title: "Lý do khen thưởng", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "CommendationFormName", title: "Hình thức khen thưởng", width: "200px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "TotalValue", title: "Tổng giá trị", width: "150px",
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
                var valueClick = e;

                $scope.model = {
                    Id: 0,
                    DecisionNumber: STRING_EMPTY,
                    DecisionDate: STRING_EMPTY,
                    ThePersonSignedTheDecisionId: STRING_EMPTY,
                    BonusDay: STRING_EMPTY,
                    RewardPlanId: STRING_EMPTY,
                    BonusGrounds: STRING_EMPTY,
                    RewardReasonId: STRING_EMPTY,
                    CommendationFormId: STRING_EMPTY,
                    BonusBudgetSourceId: STRING_EMPTY,
                    TotalValue: 0,
                    StatusBonusId: STRING_EMPTY,
                    JobPositionName: STRING_EMPTY,
                    ThePersonSignedTheDecisionAutoComplete: STRING_EMPTY,
                    ThePersonSignedTheDecisionTemp: STRING_EMPTY
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
                            bootbox.alert("<span style='color:red; text-align:justify;'>" + CANNOT_FIND_ANY_WITH_GIVEN_ID + "</span>");
                            stopLoadingProfilePage();
                        } else {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                            $scope.GetBonusById($scope.IdTemp);
                        }
                        break;
                    case "CANCEL":
                        loadingProfilePage();
                        $window.location.reload();
                        stopLoadingProfilePage();
                        break;
                    case "REFRESH":
                        loadingProfilePage();
                        RefreshKendoGrid("Table");
                        stopLoadingProfilePage();
                        break;
                    case "DELETE":
                        bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                            if (result === true) {
                                loadingProfilePage();
                                var data = angular.copy($scope.model);
                                data.Id = $scope.IdTemp;
                                BonusInformationService.DeleteBonusInformation(data).then(function success(response) {
                                    if (response.data.status === 200) {
                                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                                        RefreshKendoGrid("Table");
                                        stopLoadingProfilePage();
                                    } else {
                                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                        stopLoadingProfilePage();
                                    }
                                });
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            // Upsite
            $scope.Upsite = function () {
                $window.scrollTo(0, 0);
            };

            // Load datasource for ThePersonSignedTheDecisionId autocomplete box
            $scope.GetEmployeeForThePersonSignedTheDecision = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.model.ThePersonSignedTheDecisionAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item ThePersonSignedTheDecisionId autocomplete box
            $scope.onSelectThePersonSignedTheDecisionId = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForThePersonSignedTheDecision.data()[index];
                $scope.model.ThePersonSignedTheDecisionId = itemSelected.Id;
                $scope.model.ThePersonSignedTheDecisionAutoComplete = itemSelected.EmployeeName;
                $scope.model.JobPositionName = itemSelected.JobPositionName;
                $scope.$applyAsync();
                $("#ThePersonSignedTheDecisionId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item ThePersonSignedTheDecisionId autocomplete box
            $scope.onChangeThePersonSignedTheDecisionId = function () {
                if ($scope.model.ThePersonSignedTheDecisionAutoComplete === $scope.model.ThePersonSignedTheDecisionTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForThePersonSignedTheDecision.data().forEach(function (item) {
                    if ($scope.model.ThePersonSignedTheDecisionId === item.Id) {
                        $scope.model.ThePersonSignedTheDecisionAutoComplete = item.EmployeeName;
                        $scope.model.JobPositionName = item.JobPositionName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.model.ThePersonSignedTheDecisionId = STRING_EMPTY;
                    $scope.model.ThePersonSignedTheDecisionAutoComplete = STRING_EMPTY;
                    $scope.model.JobPositionName = STRING_EMPTY;
                }
            };

            // onChangeThePersonSignedTheDecisionIdValue
            $scope.onChangeThePersonSignedTheDecisionIdValue = function () {
                if ($scope.model.ThePersonSignedTheDecisionAutoComplete === STRING_EMPTY) {
                    $scope.model.ThePersonSignedTheDecisionId = STRING_EMPTY;
                    $scope.model.ThePersonSignedTheDecisionAutoComplete = STRING_EMPTY;
                    $scope.model.JobPositionName = STRING_EMPTY;
                }
            };

            // Save
            $scope.Save = function () {
                $scope.IsSave = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.model.DecisionNumber || $scope.model.DecisionNumber === STRING_EMPTY) {
                        $scope.showHasErrorDecisionNumber = true;
                    }
                    if (!$scope.model.DecisionDate || $scope.model.DecisionDate === STRING_EMPTY) {
                        $scope.showHasErrorDecisionDate = true;
                    }
                    if (!$scope.model.ThePersonSignedTheDecisionAutoComplete || $scope.model.ThePersonSignedTheDecisionAutoComplete === STRING_EMPTY) {
                        $scope.showHasErrorThePersonSignedTheDecisionId = true;
                    }
                    if (!$scope.model.StatusBonusId || $scope.model.StatusBonusId === STRING_EMPTY) {
                        $scope.showHasErrorStatusBonusId = true;
                    }

                    if ($scope.showHasErrorDecisionNumber === true || $scope.showHasErrorDecisionDate === true || $scope.showHasErrorThePersonSignedTheDecisionId === true || $scope.showHasErrorStatusBonusId === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.model);
                data.DecisionDate = kendo.parseDate($scope.model.DecisionDate, DATE_FORMAT);
                data.BonusDay = kendo.parseDate($scope.model.BonusDay, DATE_FORMAT);

                loading();
                BonusInformationService.SaveBonusInformation(data).then(function success(response) {
                    if (response.data.status === 200) {
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoading();
                        if (data.Id > 0) {
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
                        stopLoading();
                        if (data.Id > 0) {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                        }
                        else {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                            $scope.IdTemp = 0;
                        }
                    }
                });
            };

            // onShowMessageValidate
            function onShowMessageValidate() {
                $scope.showHasErrorDecisionNumber = false;
                $scope.showHasErrorDecisionDate = false;
                $scope.showHasErrorThePersonSignedTheDecisionId = false;
                $scope.showHasErrorStatusBonusId = false;
            }

            // onChangeValue
            $scope.onChangeValue = function (e) {
                switch (e) {
                    case "DecisionNumber":
                        if ($scope.model.DecisionNumber !== STRING_EMPTY) {
                            $scope.showHasErrorDecisionNumber = false;
                        } else {
                            $scope.showHasErrorDecisionNumber = true;
                        }
                        break;
                    case "DecisionDate":
                        if ($scope.model.DecisionDate !== STRING_EMPTY) {
                            $scope.showHasErrorDecisionDate = false;
                        } else {
                            $scope.showHasErrorDecisionDate = true;
                        }
                        break;
                    case "ThePersonSignedTheDecisionAutoComplete":
                        if ($scope.model.ThePersonSignedTheDecisionAutoComplete !== STRING_EMPTY) {
                            $scope.showHasErrorThePersonSignedTheDecisionId = false;
                        } else {
                            $scope.showHasErrorThePersonSignedTheDecisionId = true;
                        }
                        break;
                    case "StatusBonusId":
                        if ($scope.model.StatusBonusId !== STRING_EMPTY) {
                            $scope.showHasErrorStatusBonusId = false;
                        } else {
                            $scope.showHasErrorStatusBonusId = true;
                        }
                        break;
                    default:
                        onShowMessageValidate();
                        break;
                }
            };

            // ClickInformation
            $scope.ClickInformation = function (e) {
                switch (e) {
                    case "GENERAL_INFORMATION_REWARD":
                        loadingProfilePage();
                        $('.save-hide').removeClass('hide');
                        $scope.GetBonusById($scope.IdTemp);
                        break;
                    case "STAFF_WERE_COMMENDED":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        BonusInformationStaffWereCommendedService.GetAllBonusInformationStaffWereCommended($scope.IdTemp).then(function (response) {
                            $scope.GetAllBonusInformationStaffWereCommended = [];
                            if (response.status === 200) {
                                $scope.GetAllBonusInformationStaffWereCommended = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            $scope.GetBonusById = function (e) {
                BonusInformationService.GetBonusById(e).then(function (response) {
                    if (response.status === 200 && response.data.Success === true) {
                        $scope.model.Id = $scope.IdTemp;
                        $scope.model.DecisionNumber = response.data.Data.DecisionNumber;
                        $scope.model.DecisionDate = kendo.parseDate(response.data.Data.DecisionDate, DATE_FORMAT);
                        $scope.model.ThePersonSignedTheDecisionId = response.data.Data.ThePersonSignedTheDecisionId;
                        $scope.model.BonusDay = kendo.parseDate(response.data.Data.BonusDay, DATE_FORMAT);
                        $scope.model.RewardPlanId = response.data.Data.RewardPlanId;
                        $scope.model.BonusGrounds = response.data.Data.BonusGrounds;
                        $scope.model.RewardReasonId = response.data.Data.RewardReasonId;
                        $scope.model.CommendationFormId = response.data.Data.CommendationFormId;
                        $scope.model.BonusBudgetSourceId = response.data.Data.BonusBudgetSourceId;
                        $scope.model.TotalValue = response.data.Data.TotalValue;
                        $scope.model.StatusBonusId = response.data.Data.StatusBonusId;
                        $scope.model.ThePersonSignedTheDecisionAutoComplete = response.data.Data.ThePersonSignedTheDecisionName;
                        $scope.model.ThePersonSignedTheDecisionTemp = response.data.Data.ThePersonSignedTheDecisionName;
                        $scope.model.JobPositionName = response.data.Data.JobPositionName;
                        stopLoadingProfilePage();
                    }
                });
            };

        }]);

})(window.angular);
