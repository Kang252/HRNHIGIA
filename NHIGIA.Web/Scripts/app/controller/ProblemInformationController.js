(function (angular) {
    "use strict";
    hrmApp.controller('ProblemInformationController', [
        '$scope', 'ProblemInformationService', 'ListCategoryService', '$window', 'ProblemInformationRelatedStaffService',
        function ($scope, ProblemInformationService, ListCategoryService, $window, ProblemInformationRelatedStaffService) {

            $(document).ready(function () {
                $('.other_active').removeClass('active');
                $('.other_active').addClass('active');
                $('.other_toggle').removeClass('toggled');
                $('.other_toggle').addClass('toggled');
                $('.other_display').removeClass('display_block');
                $('.other_display').addClass('display_block');
                $('.problem_active').removeClass('active');
                $('.problem_active').addClass('active');
                $('.problem_toggle').removeClass('toggled');
                $('.problem_toggle').addClass('toggled');
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
                ListCategoryService.GetDataForDropdown(TypeOfIncident).then(function (response) {
                    $scope.TypeOfIncidentDropdownlist = [];
                    if (response.status === 200) {
                        $scope.TypeOfIncidentDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(WorkUnit).then(function (response) {
                    $scope.RelatedUnitDropdownlist = [];
                    if (response.status === 200) {
                        $scope.RelatedUnitDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(CompensationStatus).then(function (response) {
                    $scope.CompensationStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.CompensationStatusDropdownlist = response.data;
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
                dataSource: ProblemInformationService.GetAllProblem(),
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
                        field: "CompensationStatusName", title: "Trạng thái", width: "150px", locked: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "ProblemName", title: "Tên sự cố", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "HappenDay", title: "Ngày xảy ra", width: "150px",
                        template: "<span>#= (HappenDay == null) ? '' : kendo.toString(kendo.parseDate(HappenDay, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
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
                        field: "TypeOfIncidentName", title: "Loại sự cố", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "TotalValueOfDamage", title: "Tổng giá trị thiệt hại", width: "200px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "TotalCompensationValue", title: "Tổng giá trị được bồi thường", width: "210px",
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
                    ProblemName: STRING_EMPTY,
                    TypeOfIncidentId: STRING_EMPTY,
                    HappenDay: STRING_EMPTY,
                    WhereHappened: STRING_EMPTY,
                    Reason: STRING_EMPTY,
                    DescriptionOfTheProblem: STRING_EMPTY,
                    RelatedUnitId: STRING_EMPTY,
                    TotalValueOfDamage: 0,
                    TotalCompensationValue: 0,
                    CompensationStatusId: STRING_EMPTY
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
                            $scope.GetProblemById($scope.IdTemp);
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
                    default:
                        break;
                }
            };

            // Upsite
            $scope.Upsite = function () {
                $window.scrollTo(0, 0);
            };

            // Save
            $scope.Save = function () {
                $scope.IsSave = true;

                // Check validate form
                if ($scope.CheckValidate === true) {
                    if (!$scope.model.ProblemName || $scope.model.ProblemName === STRING_EMPTY) {
                        $scope.showHasErrorProblemName = true;
                    }
                    if (!$scope.model.TypeOfIncidentId || $scope.model.TypeOfIncidentId === STRING_EMPTY) {
                        $scope.showHasErrorTypeOfIncidentId = true;
                    }
                    if (!$scope.model.HappenDay || $scope.model.HappenDay === STRING_EMPTY) {
                        $scope.showHasErrorHappenDay = true;
                    }
                    if (!$scope.model.RelatedUnitId || $scope.model.RelatedUnitId === STRING_EMPTY) {
                        $scope.showHasErrorRelatedUnitId = true;
                    }
                    if (!$scope.model.CompensationStatusId || $scope.model.CompensationStatusId === STRING_EMPTY) {
                        $scope.showHasErrorCompensationStatusId = true;
                    }

                    if ($scope.showHasErrorProblemName === true || $scope.showHasErrorTypeOfIncidentId === true || $scope.showHasErrorHappenDay === true || $scope.showHasErrorRelatedUnitId === true || $scope.showHasErrorCompensationStatusId === true) {
                        return;
                    }
                    $scope.CheckValidate === false;
                }

                var data = angular.copy($scope.model);
                data.HappenDay = kendo.parseDate($scope.model.HappenDay, DATE_FORMAT);

                loading();
                ProblemInformationService.SaveProblemInformation(data).then(function success(response) {
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
                $scope.showHasErrorProblemName = false;
                $scope.showHasErrorTypeOfIncidentId = false;
                $scope.showHasErrorHappenDay = false;
                $scope.showHasErrorRelatedUnitId = false;
                $scope.showHasErrorCompensationStatusId = false;
            }

            // onChangeValue
            $scope.onChangeValue = function (e) {
                switch (e) {
                    case "ProblemName":
                        if ($scope.model.ProblemName !== STRING_EMPTY) {
                            $scope.showHasErrorProblemName = false;
                        } else {
                            $scope.showHasErrorProblemName = true;
                        }
                        break;
                    case "TypeOfIncidentId":
                        if ($scope.model.TypeOfIncidentId !== STRING_EMPTY) {
                            $scope.showHasErrorTypeOfIncidentId = false;
                        } else {
                            $scope.showHasErrorTypeOfIncidentId = true;
                        }
                        break;
                    case "HappenDay":
                        if ($scope.model.HappenDay !== STRING_EMPTY) {
                            $scope.showHasErrorHappenDay = false;
                        } else {
                            $scope.showHasErrorHappenDay = true;
                        }
                        break;
                    case "RelatedUnitId":
                        if ($scope.model.RelatedUnitId !== STRING_EMPTY) {
                            $scope.showHasErrorRelatedUnitId = false;
                        } else {
                            $scope.showHasErrorRelatedUnitId = true;
                        }
                        break;
                    case "CompensationStatusId":
                        if ($scope.model.CompensationStatusId !== STRING_EMPTY) {
                            $scope.showHasErrorCompensationStatusId = false;
                        } else {
                            $scope.showHasErrorCompensationStatusId = true;
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
                    case "GENERAL_INCIDENT_INFORMATION":
                        loadingProfilePage();
                        $('.save-hide').removeClass('hide');
                        $scope.GetProblemById($scope.IdTemp);
                        break;
                    case "RELATED_STAFF":
                        loadingProfilePage();
                        $('.save-hide').addClass('hide');
                        ProblemInformationRelatedStaffService.GetAllProblemInformationRelatedStaff(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllProblemInformationRelatedStaff = [];
                            if (response.status === 200) {
                                $scope.GetAllProblemInformationRelatedStaff = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            $scope.GetProblemById = function (e) {
                ProblemInformationService.GetProblemById(e).then(function (response) {
                    if (response.status === 200 && response.data.Success === true) {
                        $scope.model.Id = $scope.IdTemp;
                        $scope.model.ProblemName = response.data.Data.ProblemName;
                        $scope.model.TypeOfIncidentId = response.data.Data.TypeOfIncidentId;
                        $scope.model.HappenDay = kendo.parseDate(response.data.Data.HappenDay, DATE_FORMAT);
                        $scope.model.WhereHappened = response.data.Data.WhereHappened;
                        $scope.model.Reason = response.data.Data.Reason;
                        $scope.model.DescriptionOfTheProblem = response.data.Data.DescriptionOfTheProblem;
                        $scope.model.RelatedUnitId = response.data.Data.RelatedUnitId;
                        $scope.model.TotalValueOfDamage = response.data.Data.TotalValueOfDamage;
                        $scope.model.TotalCompensationValue = response.data.Data.TotalCompensationValue;
                        $scope.model.CompensationStatusId = response.data.Data.CompensationStatusId;
                        stopLoadingProfilePage();
                    }
                });
            };

        }]);

})(window.angular);
