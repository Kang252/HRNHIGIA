(function () {
    "use strict";
    hrmApp.service("QuitInformationService", ["$http", function ($http) {

        var service = {
            GetAllQuitInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllQuitInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveQuitInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveQuitInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteQuitInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteQuitInformationUrl,
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
