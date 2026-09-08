(function (angular) {
    "use strict";
    hrmApp.controller('BulkUploadController', [
        '$scope',
        function ($scope) {

            $(document).ready(function () {
                $('.system_active').removeClass('active');
                $('.system_active').addClass('active');
                $('.system_toggle').removeClass('toggled');
                $('.system_toggle').addClass('toggled');
                $('.system_display').removeClass('display_block');
                $('.system_display').addClass('display_block');
                $('.system_utilities_display').removeClass('display_block');
                $('.system_utilities_display').addClass('display_block');
                $('.system_utilities_active').removeClass('active');
                $('.system_utilities_active').addClass('active');
                $('.system_utilities_bulk_upload_active').removeClass('active');
                $('.system_utilities_bulk_upload_active').addClass('active');
                $('.system_utilities_bulk_upload_toggle').removeClass('toggled');
                $('.system_utilities_bulk_upload_toggle').addClass('toggled');
            });

            // DownloadTemplateListCategory
            $scope.DownloadTemplateListCategory = function () {
                $("#exportExcelFormTemplateListCategory").remove();

                var form = document.createElement("form");
                form.setAttribute("method", "post");
                form.setAttribute("action", ExportForImportListCategoryUrl);
                form.setAttribute("id", "exportExcelFormTemplateListCategory");
                form.setAttribute("target", "_blank");

                var hiddenField = document.createElement("input");
                hiddenField.setAttribute("name", "jsonRequest");
                hiddenField.setAttribute("value", "jsonRequest");
                hiddenField.setAttribute("type", "hidden");

                form.appendChild(hiddenField);
                document.body.appendChild(form);

                $("#exportExcelFormTemplateListCategory").submit();
            };

        }]);

})(window.angular);
