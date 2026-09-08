(function () {
    "use strict";
    hrmApp.service("TrainingProcessInformationService", ["$http", function ($http) {

        var service = {
            GetAllTrainingProcessInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllTrainingProcessInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveTrainingProcessInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveTrainingProcessInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteTrainingProcessInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteTrainingProcessInformationUrl,
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
