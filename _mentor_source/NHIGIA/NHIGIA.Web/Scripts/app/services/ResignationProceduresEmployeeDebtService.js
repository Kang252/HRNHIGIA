(function () {
    "use strict";
    hrmApp.service("ResignationProceduresEmployeeDebtService", ["$http", function ($http) {

        var service = {
            GetAllResignationProceduresEmployeeDebt: function (id, resignationProceduresId) {
                var response = $http({
                    method: "POST",
                    url: GetAllResignationProceduresEmployeeDebtUrl,
                    cache: false,
                    data: {
                        id: id,
                        resignationProceduresId: resignationProceduresId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveResignationProceduresEmployeeDebt: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveResignationProceduresEmployeeDebtUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteResignationProceduresEmployeeDebt: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteResignationProceduresEmployeeDebtUrl,
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
