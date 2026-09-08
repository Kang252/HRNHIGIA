(function () {
    "use strict";
    hrmApp.service("ReceiveInformationService", ["$http", function ($http) {

        var service = {
            GetAllReceiveInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllReceiveInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveReceiveInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveReceiveInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteReceiveInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteReceiveInformationUrl,
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
