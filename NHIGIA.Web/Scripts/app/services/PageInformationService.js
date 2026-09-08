(function () {
    "use strict";
    hrmApp.service("PageInformationService", ["$http", function ($http) {

        var service = {
            GetAllPageInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllPageInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SavePageInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SavePageInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeletePageInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeletePageInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },
        };

        return service;
    }]);
})();
