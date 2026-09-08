(function () {
    "use strict";
    hrmApp.service("EvaluateService", ["$http", function ($http) {

        var service = {
            GetAllEvaluate: function () {
                loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllEvaluateUrl,
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
                                stopLoading();
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

            SaveEvaluate: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEvaluateUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEvaluate: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEvaluateUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetEvaluateById: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetEvaluateByIdUrl,
                    cache: false,
                    data: {
                        id: id
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveEvaluateDetail: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEvaluateDetailUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteEvaluateDetail: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteEvaluateDetailUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            SaveEvaluateForm: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveEvaluateFormUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetEvaluateByEmployee: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetEvaluateByEmployeeUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            }
        };

        return service;
    }]);
})();
