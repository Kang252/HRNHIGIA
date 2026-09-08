(function () {
    "use strict";
    hrmApp.service("PoliticsHealthMilitaryInformationService", ["$http", function ($http) {

        var service = {
            GetAllPoliticsHealthMilitaryInformation: function (employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllPoliticsHealthMilitaryInformationUrl,
                    cache: false,
                    data: {
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SavePoliticsHealthMilitaryInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SavePoliticsHealthMilitaryInformationUrl,
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
