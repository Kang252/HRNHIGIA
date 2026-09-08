function onProcessingResultsSucessfully(e) {
    if (e.response.status == 200) {
        bootbox.alert("<span style='color:green; text-align:justify;'>" + e.response.message + "</span>", function () {
            location.reload();
        });
    }
    else {
        $("li.k-file").addClass("k-file-error");
        bootbox.alert("<span style='color:red; text-align:justify;'>" + e.response.message + "</span>");
    }
};

function validateFileSelected(e) {
    var files = e.files;
    $.each(files, function () {
        if (this.extension.toLowerCase() != ".xls" && this.extension.toLowerCase() != ".xlsx") {
            $("li.k-file").addClass("k-file-error");
            bootbox.alert("<span style='color:red; text-align:justify;'>Chỉ có thể tải lên tệp excel</span>");
            e.preventDefault();
        }
    });
};