(function () {
    "use strict";
    hrmApp.service("AttachmentInformationService", ["$http", function ($http) {

        var service = {
            GetAllAttachmentInformation: function (id, employeeId, employeesOnBusinessTripId) {
                var response = $http({
                    method: "POST",
                    url: GetAllAttachmentInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId,
                        employeesOnBusinessTripId: employeesOnBusinessTripId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveAttachmentInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveAttachmentInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteAttachmentInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteAttachmentInformationUrl,
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
