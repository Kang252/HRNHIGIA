(function () {
    "use strict";
    hrmApp.service("EmployeesOnBusinessTripService", ["$http", function ($http) {

        var service = {
            GetAllEmployeesOnBusinessTrip: function () {
                //loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllEmployeesOnBusinessTripUrl,
                                cache: false,
                                data: JSON.stringify({
                                    page: params.page,
                                    pageSize: params.pageSize,
                                    sortColumn: params.sort.split('-')[0],
                                    sortType: params.sort.split('-')[1],
                                    filterColumn: params.filter
                                }),
                                contentType: "application/json"
                            }).success(function (data) {
                                options.success(data);
                                //stopLoading();
                            }).error(function (data) {
                                options.error();
                            });
                        }
                    },
                    batch: false,
                    serverPaging: true,
                    serverSorting: true,
                    serverFiltering: true,
                    pageSize: 10,
                    schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
                });
                return response;
            },

            SaveEmployeesOnBusinessTrip: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEmployeesOnBusinessTripUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetEmployeesOnBusinessTripById: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetEmployeesOnBusinessTripByIdUrl,
                    cache: false,
                    data: {
                        id: id
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetEmployeesOnBusinessTripInformation: function (employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetEmployeesOnBusinessTripInformationUrl,
                    cache: false,
                    data: {
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEmployeesOnBusinessTrip: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEmployeesOnBusinessTripUrl,
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
