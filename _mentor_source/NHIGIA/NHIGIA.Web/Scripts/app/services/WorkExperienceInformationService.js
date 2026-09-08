(function () {
    "use strict";
    hrmApp.service("WorkExperienceInformationService", ["$http", function ($http) {

        var service = {
            GetAllWorkExperienceInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllWorkExperienceInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveWorkExperienceInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveWorkExperienceInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteWorkExperienceInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteWorkExperienceInformationUrl,
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
