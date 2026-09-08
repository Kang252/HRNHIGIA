(function () {
    "use strict";
    hrmApp.service("ResignationProceduresService", ["$http", function ($http) {

        var service = {
            GetResignationProcedures: function () {
                //loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetResignationProceduresUrl,
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

            SaveResignationProcedures: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveResignationProceduresUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteResignationProcedures: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteResignationProceduresUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetGeneralInformationForResignationProcedures: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetGeneralInformationForResignationProceduresUrl,
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
