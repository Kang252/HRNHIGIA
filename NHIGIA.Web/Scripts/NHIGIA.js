var commonErrorMessage = "Error. Please obtain a screenshot and log issue with IT Services.";
var confirmDeleteMessage = "Please confirm if you really want to delete this item.";
var confirmDeleteTitle = "Confirm Delete";
var pageCode = "";
var getEntityIdAndYaByEntityCaseIdUrl = "";
var getEntityInformationUrl = "";
var userGrade = "";

//URL schedule
var profitandlossUrl = "";
var freeexceldataUrl = "";
var defaultUrl = "";
//URL schedule


function onKendoGridEdit(event) {
    var title = event.model.isNew() ? "Insert" : "Update";
    event.container.parent().find(".k-window-title").text(title);
    event.container.parent().find(".k-edit-buttons .k-grid-update").html("<span class='k-icon k-update'></span>" + title);
    if (!event.model.isNew()) {
        event.container.parent().find('#ScheduleGroup_ScheduleGroupCode').attr('readonly', 'readonly');
        event.container.parent().find('#ScheduleType_ScheduleTypeCode').attr('disabled', 'disabled');
    }
}

function onGridRequestEnd(gridName) {
    return function (e) {
        setElementBusyIndicator(".k-widget.k-window", false);
        if (e.type === "create" || e.type === "destroy" || e.type === "update") {
            $("#" + gridName).data("kendoGrid").dataSource.read();

        }
    }
};

function onGridRequestStart(e) {
    if (e.type === "create" || e.type === "destroy" || e.type === "update") {
        setElementBusyIndicator(".k-widget.k-window", true);
    }
};

function onGridError(gridName) {
    return function (e) {
        handleAjaxError(e);
        $("#" + gridName).data("kendoGrid").cancelChanges();
    }
};

function handleAjaxError(error) {
    if (error.xhr.status === 403 || error.xhr.status === 401) {
        openMessageWindow("Error", error.xhr.statusText);
    } else {
        openMessageWindow("Error", commonErrorMessage);
    }
}

function deleteRow(e) {
    e.preventDefault();
    var grid = this;
    var row = this.dataItem($(e.currentTarget).closest("tr"));
    openConfirmWindow(confirmDeleteTitle, confirmDeleteMessage, function () {
        grid.dataSource.remove(row);
        grid.dataSource.sync();
    });
};

function setElementBusyIndicator(element, isBusy) {
    kendo.ui.progress($(element), isBusy);
    //$("html, body").css("overflow", isBusy ? "hidden" : "");
}

function setBusyIndicator(id, isBusy) {
    setElementBusyIndicator("#" + id, isBusy);
}

function setBodyBusyIndicator(isBusy) {
    setBusyIndicator("cto-body", isBusy);
}

function toggleRequiredFieldError($el) {
    if (scheduleTypeCode !== 'Provision' && scheduleTypeCode !== 'ProvisionBonus') {
        if ($el.val().trim().length === 0) {
            //if input is add new section is return
            if ($el.attr("placeholder") === "Add new section") return;

            $el.addClass("ctct-col-text-input-error").removeClass("ctct-col-text-input-error-no-red");
            if ($el.parents(".ctct-col-textbox").length == 0) {
                $el.attr("placeholder", "This is a required field");
            }
        } else {
            $el.removeClass("ctct-col-text-input-error");
        }
    }
    else {
        if ($el.val().trim().length === 0) {
            $el.addClass("ctct-col-text-input-error").removeClass("ctct-col-text-input-error-no-red");
            if ($el.parents(".ctct-col-textbox").length == 0) {
                $el.attr("placeholder", "This is a required field");
            }
        } else {
            $el.removeClass("ctct-col-text-input-error");
        }
    }
}

function validateRequiredFields($parent) {

    $parent.find(":input.required").each(function () {
        toggleRequiredFieldError($(this));
    });

    return (!$parent.find(":input.ctct-col-text-input-error").length);
}

$(document).ready(function () {

   

    //Set max width for header bar
    var menuLength = $('.ctn-right > *').length;
    $('.ctn-right > *').each(function () {
        $(this).css('max-width', "calc(100% / " + menuLength + " + 20px");
    })
    $(document).on("focus", ":input.required", function () {
        $(this).attr('before-value', $(this).val().trim());
    });
    $(document).on("change focusout", ":input.required", function () {
        toggleRequiredFieldError($(this));
    });

    // enable multiple selection in Listbox without pressing Ctrl key
    $("select[multiple]").on("mousedown", "option", function (e) {
        e.preventDefault();
        $(this).prop("selected", $(this).prop("selected") ? false : true);
        return false;
    });

    //____________________________________________main dashboard - toggle collapse/expand block_________________________________________________

    $(".cdt-block > p > a").click(function () {
        $(this).parents("p").next().toggleClass("active");
        $(this).find("i.fa").toggleClass("active");
    });
    //_______________________________bind tooltip for any popover tooltip
    $('*[data-toggle=popover]').popover({
        placement: 'top',
        trigger: 'hover'
    });

    if (window.location.pathname && window.location.pathname.toLowerCase().indexOf("profitandloss") == -1 && window.location.pathname.toLowerCase().indexOf("mapping") == -1) {
        if (sessionStorage.getItem("Page2Visited")) {
            sessionStorage.removeItem("Page2Visited");
        }
    }
    //disable add new case with grade restricted user
    disableAddNewCase();

    // Opera 8.0+
    var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;

    // Firefox 1.0+
    var isFirefox = typeof InstallTrigger !== 'undefined';

    // Safari 3.0+ "[object HTMLElementConstructor]" 
    var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || (typeof safari !== 'undefined' && safari.pushNotification));

    // Internet Explorer 6-11
    var isIE = /*@cc_on!@*/false || !!document.documentMode;

    // Edge 20+
    var isEdge = !isIE && !!window.StyleMedia;

    // Chrome 1 - 71
    var isChrome = !!window.chrome && !!window.chrome.runtime;

    // Blink engine detection
    var isBlink = (isChrome || isOpera) && !!window.CSS;
    if (!isChrome && !isEdge) {
        $('.kit-noti').show();
        $('.cto-top-nav').addClass('active');
        $('.main-body').addClass('active');
    }
});

//________________________Autocomplete search box____________________//
var textSearch;

function GetSearchResult() {
    textSearch = $("#EntitySearch").val();
    return { keyword: $("#EntitySearch").val() };
}

function onEntitySearchSelect(e) {
    var dataItem = this.dataItem(e.item.index());

    $.ajax({
        type: "POST",
        url: getEntityInformationUrl,
        data: {
            entityCaseId: dataItem.EntityCaseId,
            entityId: dataItem.EntityId,
            entityGroupId: dataItem.EntityGroupId
        },
        success: function (response) {
            if (response.Entity == null) {
                window.location.href = createCaseUrl + "?entityId=" + dataItem.EntityId
                        + "&pageCode=" + pageCode + "&groupId=" + dataItem.EntityGroupId;
            } else {
                if (dataItem.YearOfAssessment === null || dataItem.EntityCaseId === 0) {
                    window.location.href = createCaseUrl + "?entityId=" + dataItem.EntityId
                        + "&pageCode=" + pageCode + "&groupId=" + dataItem.EntityGroupId;
                } else {
                    var yaParam = (dataItem.YearOfAssessment === null) ? "" : "&ya=" + dataItem.YearOfAssessment;
                    var caseIdParam = (dataItem.EntityCaseId === 0) ? "" : "&entityCaseId=" + dataItem.EntityCaseId;

                    window.location.href = entityDashboardUrl + "?entityId=" + dataItem.EntityId
                                                       + yaParam + "&groupId=" + dataItem.EntityGroupId + caseIdParam;
                }
            }
        },
        error: function (ts) {
            openMessageWindow("Message", FileNotFound);
        },
        complete: function () {
        }
    });




}

//________________________End autocomplete search______________________//

function OnlyNumeric(id) {
    $(id).keydown(function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
            // Allow: Ctrl+A, Command+A
            (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });
};

// ESC key Press handler
$(function () {
    kendo.ui.Window.fn._keydown = function (originalFn) {
        var keyEsc = 27;
        return function (e) {
            if (e.which !== keyEsc) {
                originalFn.call(this, e);
            }
        };
    }(kendo.ui.Window.fn._keydown);
});

// Refresh Kendo Grid (by ID)
function RefreshKendoGrid(id) {
    $("#" + id).data("kendoGrid").dataSource.page(1);
}

function getUrlParameter(sParam) {
    var sPageUrl = decodeURIComponent(window.location.search.substring(1)),
        sUrlVariables = sPageUrl.split("&"),
        sParameterName;

    for (var i = 0; i < sUrlVariables.length; i++) {
        sParameterName = sUrlVariables[i].split("=");

        if (sParameterName[0].toLowerCase() === sParam.toLowerCase()) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
};

function disableAddNewCase() {
    if (userGrade == 'Restrict' && $("#CaseGrid a.k-button-icontext").length == 1) {
        $("#CaseGrid a.k-button-icontext").addClass("disabled", "");
    }
    else {
        $("#CaseGrid a.k-button-icontext").removeClass("disabled");
    }
}

