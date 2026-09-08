(function (angular) {
    "use strict";
    hrmApp.controller('ListCategoryController', [
        '$scope', 'ListCategoryService', '$window',
        function ($scope, ListCategoryService, $window) {

            $(document).ready(function () {
                $('.list_category').removeClass('active');
                $('.list_category').addClass('active');
            });

            // Define 
            $scope.IsSave = false;
            $scope.formErrors = {};
            $scope.CheckDuplicate = false;

            $scope.IdTemp = 0;
            $scope.IdTempEdit = 0;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;
            $scope.MessageErrorForDuplicate = STRING_EMPTY;

            // onChange
            function onChange(arg) {
                $scope.IdTemp = arg.sender.dataItem(arg.sender.select()).Id;
                $scope.IdTempEdit = arg.sender.dataItem(arg.sender.select()).Id;
                $("#ButtonCreate").prop("disabled", false);
                $("#ButtonDelete").prop("disabled", false);
            }

            // onDisable
            function onDisable() {
                $("#ButtonCreate").prop("disabled", true);
                $("#ButtonDelete").prop("disabled", true);
                $scope.IdTemp = 0;
            }

            // Designer gird
            $scope.Section = {
                dataSource: ListCategoryService.GetAllListCategory(),
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
                        field: "Id", title: "Id", width: "40px",
                        filterable: false, sortable: false
                    },
                    {
                        field: "Code", title: "Mã", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "Name", title: "Tên danh mục", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "Address", title: "Địa chỉ", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "ListCategoryTypeName", title: "Loại danh mục", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    }

                ]
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
                $scope.CheckDuplicate = false;
                var valueClick = e;
                $("#btnSave").prop("disabled", false);
                $("#btnClose").prop("disabled", false);

                $scope.model = {
                    Id: 0,
                    Code: STRING_EMPTY,
                    Name: STRING_EMPTY,
                    Address: STRING_EMPTY,
                    ListCategoryTypeId: STRING_EMPTY,
                    CreatedDate: STRING_EMPTY,
                    CreatedBy: STRING_EMPTY,
                    ModifiedDate: STRING_EMPTY,
                    ModifiedBy: STRING_EMPTY,
                    IsDeleted: false
                };

                var windowListCategory = $("#KenWindown").kendoWindow({
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
                    case "ADD":
                        loadingPopUp();
                        windowListCategory.title("Thêm mới");
                        $scope.Valid.$setUntouched();
                        windowListCategory.open();
                        windowListCategory.center();
                        stopLoadingPopUp();
                        break;
                    case "EDIT":
                        loadingPopUp();
                        ListCategoryService.GetListCategoryById($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.Success === true) {
                                $scope.model.Id = $scope.IdTemp;
                                $scope.model.Code = response.data.Data.Code;
                                $scope.model.Name = response.data.Data.Name;
                                $scope.model.Address = response.data.Data.Address;
                                $scope.model.ListCategoryTypeId = response.data.Data.ListCategoryTypeId;
                                windowListCategory.title("Sửa dữ liệu");
                                windowListCategory.open();
                                windowListCategory.center();
                                $scope.IdTemp = 0;
                                stopLoadingPopUp();
                            } else {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                $scope.IdTemp = 0;
                                stopLoadingPopUp();
                            }
                        });
                        break;
                    case "DELETE":
                        if ($scope.IdTemp > 0) {
                            bootbox.confirm(MSG_DELETED_CONFIRM, function (result) {
                                if (result === true) {
                                    loading();
                                    var data = angular.copy($scope.model);
                                    data.Id = $scope.IdTemp;
                                    ListCategoryService.DeleteListCategory(data).then(function success(response) {
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
                            var data = angular.copy($scope.model);
                            ListCategoryService.DeleteListCategory(data).then(function success(response) {
                                if (response.data.status === 404) {
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    stopLoading();
                                }
                            });
                        }
                        break;
                    case "REFRESH":
                        loading();
                        RefreshKendoGrid("Table");
                        stopLoading();
                        break;
                    case "EXPORTEXCEL":
                        $("#exportExcelForm").remove();

                        var form = document.createElement("form");
                        form.setAttribute("method", "post");
                        form.setAttribute("action", ExportExcelUrl);
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
                    default:
                        break;
                }
            };

            // BindDataListCategoryType
            function BindDataListCategoryType() {
                ListCategoryService.GetListCategoryType().then(function (response) {
                    $scope.ListCategoryTypeDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ListCategoryTypeDropdownlist = response.data;
                    }
                });
            }
            BindDataListCategoryType();

            // Save
            $scope.Save = function (form) {
                $scope.IsSave = true;
                if (form !== null && !form.$valid) {
                    return;
                }

                loadingPopUp();
                $("#btnSave").prop("disabled", true);
                $("#btnClose").prop("disabled", true);
                var data = angular.copy($scope.model);
                ListCategoryService.SaveListCategory(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindown").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        RefreshKendoGrid("Table");
                        stopLoadingPopUp();
                        $scope.IdTemp = 0;
                    } else if (response.data.status === 409) {
                        $scope.CheckDuplicate = true;
                        $("#btnSave").prop("disabled", false);
                        $("#btnClose").prop("disabled", false);
                        $scope.MessageErrorForDuplicate = $scope.model.Name + response.data.message;
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindown").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                        $scope.IdTemp = 0;
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
