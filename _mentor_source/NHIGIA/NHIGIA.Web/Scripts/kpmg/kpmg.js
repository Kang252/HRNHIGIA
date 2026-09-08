var CommonError = "Error. Please obtain a screenshot and log issue with IT Services.";

function PropertyKendoEnable(id, type, condition) {
    $("[requiredfor='" + id + "']").removeClass("input-required");
    if (type !== "") {
        var kendoData = $("#" + id).data("kendo" + type);
        kendoData.enable(condition);

        if (!condition) {
            kendoData.wrapper.addClass("input-disable");
            kendoData.value(null);
        } else {
            kendoData.wrapper.removeClass("input-disable");
        }

    } else {
        if (!condition) {
            $("#" + id).attr({ "readonly": "readonly" }).addClass("input-disable");
            $("#" + id).val(null);
        } else {
            $("#" + id).removeAttr("readonly").removeClass("input-disable");
        }
    }
}
function RefreshKendoGrid(id) {
    $("#" + id).data("kendoGrid").dataSource.read();
}
function KendoDataType(id, type) {
    return $("#" + id).data("kendo" + type);
}
var rowCount = {};
function RowNumber(gridId) {
    if (rowCount[gridId] === undefined)
        rowCount[gridId] = 0;
    var count = ++rowCount[gridId];
    if (rowCount[gridId] === KendoDataType(gridId, "Grid").dataSource.view().length)
        rowCount[gridId] = 0;
    return count;
}
function RequiredDataValidate(id, type, requiredInputs, condition, validControls) {
    if (validControls) {
        if (type !== "" && (!KendoDataType(id, type).value() || KendoDataType(id, type).value() === "") && condition) {
            requiredInputs.push(id);
            $("[requiredfor='" + id + "']").addClass("input-required");
        } else if (($("#" + id).val().length <= 0 || $("#" + id).val() === "") && condition) {
            requiredInputs.push(id);
            $("[requiredfor='" + id + "']").addClass("input-required");
        } else {
            $("[requiredfor='" + id + "']").removeClass("input-required");
        }
    } else {
        if (condition) {
            requiredInputs.push(id);
            $("[requiredfor='" + id + "']").addClass("input-required");
        } else {
            $("[requiredfor='" + id + "']").removeClass("input-required");
        }
    }
}

function GetCheckboxData(name) {
    var checkboxData = [];
    $("input[name='" + name + "[]']:checked").each(function () { checkboxData.push($(this).val()); });
    return checkboxData;
}
function CheckAllCheckBox(source, name) {
    $("input[name='" + name + "[]']")
        .each(function () {
            $(this).prop("checked", $(source).is(":checked"));
        });
}
function BindIsSelectedToCheckBox(gridName) {
    $('#' + gridName).on('click', '.checkbox', function () {
        var checked = $(this).is(':checked');
        var grid = KendoDataType(gridName, "Grid");
        var datasource = grid.dataSource.data();
        var dataItem = grid.dataItem($(this).closest('tr'));
        var selectedRows = $("#" + gridName).find("tr.k-state-selected");
        dataItem.set('IsSelected', checked);
        for (var i = 0; i < datasource.length; i++) {
            if (datasource[i].IsSelected)
                $(document).find("tr[data-uid='" + datasource[i].uid + "']").each(function () {
                    if (!$(this).hasClass("k-state-selected"))
                        $(this).addClass("k-state-selected");
                });
        }
        selectedRows.each(function () {
            var uid = $(this).attr("data-uid");
            if (uid !== null && uid != undefined && uid !== dataItem.uid) {
                $(document).find("tr[data-uid='" + uid + "']").each(function () {
                    if (!$(this).hasClass("k-state-selected"))
                        $(this).addClass("k-state-selected");
                });
            }
        });
    });
}

function RowClicked(e) {
    e.preventDefault();
    var gridName = this.select().closest(".k-grid").attr("id");
    var grid = KendoDataType(gridName, "Grid");
    if (grid === null || grid == undefined)
        return false;
    var datasource = grid.dataSource.data();
    if (datasource.length <= 0)
        return false;
    for (var i = 0; i < datasource.length; i++) {
        if (datasource[i].IsSelected)
            $(document).find("tr[data-uid='" + datasource[i].uid + "']").each(function () {
                if (!$(this).hasClass("k-state-selected"))
                    $(this).addClass("k-state-selected");
            });
    }
}

function CheckAll(ele, gridName) {
    var state = $(ele).is(':checked');
    var grid = KendoDataType(gridName, "Grid");
    $.each(grid.dataSource.view(), function () {
        if (this['IsSelected'] !== state)
            this.dirty = true;
        this['IsSelected'] = state;
    });
    grid.refresh();
    var dataSource = grid.dataSource.data();
    for (var i = 0; i < dataSource.length; i++) {
        if (dataSource[i].IsSelected)
            $(document).find("tr[data-uid='" + dataSource[i].uid + "']").each(function () {
                if (!$(this).hasClass("k-state-selected"))
                    $(this).addClass("k-state-selected");
            });
    }
}

function GetSelectedItems(gridName, propName) {
    var items = KendoDataType(gridName, "Grid").dataSource.data();
    var result = [];
    if (items.length === 0) return result;

    for (var i = 0; i < items.length; i++) {
        if (items[i].IsSelected) {
            result.push(items[i][propName]);
        }
    }

    return result;
}
function GetAllItems(gridName, propName) {
    var items = KendoDataType(gridName, "Grid").dataSource.data();
    var result = [];
    if (items.length === 0) return result;

    for (var i = 0; i < items.length; i++) {
        result.push(items[i][propName]);
    }

    return result;
}

function togglePanel(ele) {
    $(ele).find("span").toggleClass('glyphicon-minus glyphicon-plus');
}
function dateFromChange(start, end) {
    var endPicker = KendoDataType(end, "DatePicker"),
        startDate = start.value();

    if (startDate) {
        startDate = new Date(startDate);
        startDate.setDate(startDate.getDate());
        endPicker.min(startDate);
    }
}
function dateToChange(start, end) {
    var startPicker = KendoDataType(start, "DatePicker"),
        endDate = end.value();

    if (endDate) {
        endDate = new Date(endDate);
        endDate.setDate(endDate.getDate());
        startPicker.max(endDate);
    }
}

function isEmptyOrWhitespace(str) {
    return str === null || str.match(/^ *$/) !== null;
}

function requestLoading() {
    kendo.ui.progress($(".layout_homepage"), true);
}

function requestLoadingDone() {
    kendo.ui.progress($(".layout_homepage"), false);
}

function showMessage(title, message) {
    bootbox.alert({
        message: message,
        title: title
    });
}

function showSuccessMessage(message) {
    showMessage("Success", message);
}

function showErrorMessage(message) {
    showMessage("Error", message);
}

function CheckDateFrom(from, to, dateErrorMsg) {
    var dateTo = KendoDataType(to, "DatePicker").value(),
        dateFrom = from.value();
    if (dateFrom === null) {
        from.value("");
    }
    if (dateFrom !== null && dateTo !== null && new Date(dateTo) < new Date(dateFrom)) {
        showErrorMessage(dateErrorMsg);
        return;
    }
}
function CheckDateTo(from, to, dateErrorMsg) {
    var dateFrom = KendoDataType(from, "DatePicker").value(),
        dateTo = to.value();
    if (dateTo === null) {
        to.value("");
    }

    if (dateFrom !== null && dateTo !== null && new Date(dateTo) < new Date(dateFrom)) {
        showErrorMessage(dateErrorMsg);
        return;
    }
}