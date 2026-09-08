(function () {
    "use strict";
    hrmApp.service("BonusInformationService", ["$http", function ($http) {

        var service = {
            GetAllBonus: function () {
                //loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllBonusUrl,
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

            GetAllBonusInformation: function (employeeId) {
                var response = $http({
                    method: "POST",
                    url: GetAllBonusInformationUrl,
                    cache: false,
                    data: {
                        employeeId: employeeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveBonusInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveBonusInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteBonusInformation: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteBonusInformationUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetBonusById: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetBonusByIdUrl,
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
