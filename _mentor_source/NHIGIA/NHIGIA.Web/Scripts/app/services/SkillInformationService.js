(function () {
    "use strict";
    hrmApp.service("SkillInformationService", ["$http", function ($http) {

        var service = {
            GetAllSkillInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllSkillInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveSkillInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveSkillInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteSkillInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteSkillInformationUrl,
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
