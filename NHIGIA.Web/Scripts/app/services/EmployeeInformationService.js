(function () {
    "use strict";
    hrmApp.service("EmployeeInformationService", ["$http", function ($http) {

        var service = {
            GetAllEmployeeInformation: function () {
                //loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllEmployeeInformationUrl,
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

            SaveProfile: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveProfileUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetEmployeeInformationById: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetEmployeeInformationByIdUrl,
                    cache: false,
                    data: {
                        id: id
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetEmployeeForAutoCompleBox: function (keyword) {
                var response = $http({
                    method: "POST",
                    url: GetEmployeeForAutoCompleBoxUrl,
                    cache: false,
                    data: { keyword: keyword },
                    contentType: "application/json"
                });
                return response;
            },

            GetEmployee: function (id, type) {
                //loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetEmployeeUrl,
                                cache: false,
                                data: JSON.stringify({
                                    id: id,
                                    type: type,
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
        };

        return service;
    }]);
})();
