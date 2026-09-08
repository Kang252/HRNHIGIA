(function () {
    "use strict";
    hrmApp.service("EmployeesOnBusinessTripAdvancesService", ["$http", function ($http) {

        var service = {
            GetAllEmployeesOnBusinessTripAdvances: function (id, employeesOnBusinessTripId) {
                var response = $http({
                    method: "POST",
                    url: GetAllEmployeesOnBusinessTripAdvancesUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeesOnBusinessTripId: employeesOnBusinessTripId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveEmployeesOnBusinessTripAdvances: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEmployeesOnBusinessTripAdvancesUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEmployeesOnBusinessTripAdvances: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEmployeesOnBusinessTripAdvancesUrl,
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
