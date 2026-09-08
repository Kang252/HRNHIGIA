(function () {
    "use strict";
    hrmApp.service("AllowanceInformationService", ["$http", function ($http) {

        var service = {
            GetAllAllowanceInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllAllowanceInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveAllowanceInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveAllowanceInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteAllowanceInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteAllowanceInformationUrl,
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
