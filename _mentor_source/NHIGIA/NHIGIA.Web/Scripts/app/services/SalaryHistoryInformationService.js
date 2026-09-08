(function () {
    "use strict";
    hrmApp.service("SalaryHistoryInformationService", ["$http", function ($http) {

        var service = {
            GetAllSalaryHistoryInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllSalaryHistoryInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveSalaryHistoryInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveSalaryHistoryInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteSalaryHistoryInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteSalaryHistoryInformationUrl,
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
