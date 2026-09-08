(function () {
    "use strict";
    hrmApp.service("EmployeesOnBusinessTripAssignedStaffService", ["$http", function ($http) {

        var service = {
            GetAllEmployeesOnBusinessTripAssignedStaff: function (employeesOnBusinessTripId) {
                var response = $http({
                    method: "POST",
                    url: GetAllEmployeesOnBusinessTripAssignedStaffUrl,
                    cache: false,
                    data: {
                        employeesOnBusinessTripId: employeesOnBusinessTripId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveEmployeesOnBusinessTripAssignedStaff: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEmployeesOnBusinessTripAssignedStaffUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEmployeesOnBusinessTripAssignedStaff: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEmployeesOnBusinessTripAssignedStaffUrl,
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
