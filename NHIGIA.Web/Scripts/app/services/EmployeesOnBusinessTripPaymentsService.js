(function () {
    "use strict";
    hrmApp.service("EmployeesOnBusinessTripPaymentsService", ["$http", function ($http) {

        var service = {
            GetAllEmployeesOnBusinessTripPayments: function (id, employeesOnBusinessTripId) {
                var response = $http({
                    method: "POST",
                    url: GetAllEmployeesOnBusinessTripPaymentsUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeesOnBusinessTripId: employeesOnBusinessTripId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveEmployeesOnBusinessTripPayments: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEmployeesOnBusinessTripPaymentsUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEmployeesOnBusinessTripPayments: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEmployeesOnBusinessTripPaymentsUrl,
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
