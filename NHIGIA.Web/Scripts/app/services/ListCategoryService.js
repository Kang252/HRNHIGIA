(function () {
    "use strict";
    hrmApp.service("ListCategoryService", ["$http", function ($http) {

        var service = {
            GetAllListCategory: function () {
                loading();
                var response = new kendo.data.DataSource({
                    type: "json",
                    transport: {
                        read: function (options) {
                            var webapi = new kendo.data.transports.webapi({ prefix: "" });
                            var params = webapi.parameterMap(options.data);
                            $http({
                                method: "POST",
                                url: GetAllListCategoryUrl,
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

            GetListCategoryById: function (id) {
                var response = $http({
                    method: "POST",
                    url: GetListCategoryByIdUrl,
                    cache: false,
                    data: {
                        id: id
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetListCategoryType: function () {
                var response = $http({
                    method: "GET",
                    url: GetListCategoryTypeUrl,
                    cache: false,
                    data: {},
                    contentType: "application/json"
                });
                return response;
            },

            SaveListCategory: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveListCategoryUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteListCategory: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteListCategoryUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            GetDataForDropdown: function (listCategoryTypeId) {
                var response = $http({
                    method: "POST",
                    url: GetDataForDropdownUrl,
                    cache: false,
                    data: {
                        listCategoryTypeId: listCategoryTypeId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetAllNationality: function () {
                var response = $http({
                    method: "GET",
                    url: GetAllNationalityUrl,
                    cache: false,
                    data: {},
                    contentType: "application/json"
                });
                return response;
            },

            GetAllProvinceCity: function (nationalityId) {
                var response = $http({
                    method: "POST",
                    url: GetAllProvinceCityUrl,
                    cache: false,
                    data: {
                        nationalityId: nationalityId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetAllDistrict: function (provinceCityId) {
                var response = $http({
                    method: "POST",
                    url: GetAllDistrictUrl,
                    cache: false,
                    data: {
                        provinceCityId: provinceCityId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetAllWards: function (districtId) {
                var response = $http({
                    method: "POST",
                    url: GetAllWardsUrl,
                    cache: false,
                    data: {
                        districtId: districtId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            GetStatusForDropdown: function (type) {
                var response = $http({
                    method: "POST",
                    url: GetStatusForDropdownUrl,
                    cache: false,
                    data: {
                        type: type
                    },
                    contentType: "application/json"
                });
                return response;
            }
        };

        return service;
    }]);
})();
