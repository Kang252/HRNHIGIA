(function () {
    "use strict";
    hrmApp.service("CertificateInformationService", ["$http", function ($http) {

        var service = {
            GetAllCertificateInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllCertificateInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveCertificateInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveCertificateInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteCertificateInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteCertificateInformationUrl,
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
