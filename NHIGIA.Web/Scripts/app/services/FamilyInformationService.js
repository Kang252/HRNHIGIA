(function () {
    "use strict";
    hrmApp.service("FamilyInformationService", ["$http", function ($http) {

        var service = {
            GetAllFamilyInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllFamilyInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveFamilyInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveFamilyInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteFamilyInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteFamilyInformationUrl,
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
