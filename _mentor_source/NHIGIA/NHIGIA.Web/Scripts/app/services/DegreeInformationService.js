(function () {
    "use strict";
    hrmApp.service("DegreeInformationService", ["$http", function ($http) {

        var service = {
            GetAllDegreeInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllDegreeInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveDegreeInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveDegreeInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteDegreeInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteDegreeInformationUrl,
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
