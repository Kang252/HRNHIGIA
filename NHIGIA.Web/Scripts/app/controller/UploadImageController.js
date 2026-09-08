(function (angular) {
    "use strict";
    hrmApp.controller('UploadImageController', [
        '$scope',
        function ($scope) {

            $(document).ready(function () {
                var finder = new CKFinder();
                finder.create();
            });
        }]);

})(window.angular);
