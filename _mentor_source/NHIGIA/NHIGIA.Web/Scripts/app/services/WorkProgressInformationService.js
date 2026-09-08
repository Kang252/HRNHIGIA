(function () {
    "use strict";
    hrmApp.service("WorkProgressInformationService", ["$http", function ($http) {

        var service = {
            GetAllWorkProgressInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllWorkProgressInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveWorkProgressInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveWorkProgressInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteWorkProgressInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteWorkProgressInformationUrl,
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
