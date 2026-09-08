(function () {
    "use strict";
    hrmApp.service("ProblemInformationRelatedStaffService", ["$http", function ($http) {

        var service = {
            GetAllProblemInformationRelatedStaff: function (id, problemInformationId) {
                var response = $http({
                    method: "POST",
                    url: GetAllProblemInformationRelatedStaffUrl,
                    cache: false,
                    data: {
                        id: id,
                        problemInformationId: problemInformationId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveProblemInformationRelatedStaff: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveProblemInformationRelatedStaffUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteProblemInformationRelatedStaff: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteProblemInformationRelatedStaffUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            SaveOnlyProblemInformationRelatedStaff: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveOnlyProblemInformationRelatedStaffUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetAllProblemInformationTrackEmployeeCompensation: function (problemInformationId, employeeId, type) {
                var response = $http({
                    method: "POST",
                    url: GetAllProblemInformationTrackEmployeeCompensationUrl,
                    cache: false,
                    data: {
                        problemInformationId: problemInformationId,
                        employeeId: employeeId,
                        type: type
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveProblemInformationTrackEmployeeCompensation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveProblemInformationTrackEmployeeCompensationUrl,
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
