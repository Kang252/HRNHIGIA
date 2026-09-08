(function () {
    "use strict";
    hrmApp.service("BonusInformationStaffWereCommendedService", ["$http", function ($http) {

        var service = {
            GetAllBonusInformationStaffWereCommended: function (bonusInformationId) {
                var response = $http({
                    method: "POST",
                    url: GetAllBonusInformationStaffWereCommendedUrl,
                    cache: false,
                    data: {
                        bonusInformationId: bonusInformationId
                    },
                    contentType: "application/json"
                });
                return response;
            },

            SaveBonusInformationStaffWereCommended: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveBonusInformationStaffWereCommendedUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            DeleteBonusInformationStaffWereCommended: function (data) {
                var response = $http({
                    method: "POST",
                    url: DeleteBonusInformationStaffWereCommendedUrl,
                    cache: false,
                    data: data,
                    contentType: "application/json"
                });
                return response;
            },

            SaveOnlyBonusInformationStaffWereCommended: function (data) {
                var response = $http({
                    method: "POST",
                    url: SaveOnlyBonusInformationStaffWereCommendedUrl,
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
