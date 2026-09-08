(function () {
    "use strict";
    hrmApp.service("AssetInformationService", ["$http", function ($http) {

        var service = {
            GetAllAssetInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllAssetInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveAssetInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveAssetInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteAssetInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteAssetInformationUrl,
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
