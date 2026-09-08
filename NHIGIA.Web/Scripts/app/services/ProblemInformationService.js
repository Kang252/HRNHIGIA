(function () {
    "use strict";
    hrmApp.service("ProblemInformationService", ["$http", function ($http) {

        var service = {
            GetAllProblem: function () {
                //loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllProblemUrl,
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

            GetAllProblemInformation: function (id, employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllProblemInformationUrl,
                    cache: false,
                    data: {
                        id: id,
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveProblemInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveProblemInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteProblemInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteProblemInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetProblemById: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetProblemByIdUrl,
                    cache: false,
                    data: {
                        id: id
                    },
                    contentType: "application/json"
                });
                return response;
            }
        };

        return service;
    }]);
})();
