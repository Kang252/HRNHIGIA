(function () {
    "use strict";
    hrmApp.service("SkinInformationService", ["$http", function ($http) {

        var service = {
            GetAllSkinInformation: function (employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllSkinInformationUrl,
                    cache: false,
                    data: {
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveSkinInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveSkinInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            }
        };

        return service;
    }]);
})();
