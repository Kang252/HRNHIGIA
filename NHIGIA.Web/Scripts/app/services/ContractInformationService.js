(function () {
    "use strict";
    hrmApp.service("ContractInformationService", ["$http", function ($http) {

        var service = {
            GetAllContractInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllContractInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveContractInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveContractInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteContractInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteContractInformationUrl,
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
