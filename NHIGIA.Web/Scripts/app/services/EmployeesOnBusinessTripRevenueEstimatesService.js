(function () {
    "use strict";
    hrmApp.service("EmployeesOnBusinessTripRevenueEstimatesService", ["$http", function ($http) {

        var service = {
            GetAllEmployeesOnBusinessTripRevenueEstimates: function (id, employeesOnBusinessTripId) {
                var response = $http({
                    method: "POST",
                    url: GetAllEmployeesOnBusinessTripRevenueEstimatesUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeesOnBusinessTripId: employeesOnBusinessTripId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveEmployeesOnBusinessTripRevenueEstimates: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEmployeesOnBusinessTripRevenueEstimatesUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEmployeesOnBusinessTripRevenueEstimates: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEmployeesOnBusinessTripRevenueEstimatesUrl,
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
