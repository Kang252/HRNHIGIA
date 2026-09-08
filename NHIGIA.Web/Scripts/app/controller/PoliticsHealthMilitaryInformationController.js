(function (angular) {
    "use strict";
    hrmApp.controller('PoliticsHealthMilitaryInformationController', [
        '$scope',
        function ($scope) {

            // Define
            $scope.IsSavePoliticsHealthMilitary = false;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;

            $scope.EmployeeTemp = $scope.$parent.IdTemp;

            // onChangeIsAUnionMember
            $scope.onChangeIsAUnionMember = function () {
                $scope.modelPoliticsHealthMilitary.DayToUnion = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.GroupPositionId = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.PlaceOfUnionAdmission = STRING_EMPTY;
            };

            // onChangeAsASsoldier
            $scope.onChangeAsASsoldier = function () {
                $scope.modelPoliticsHealthMilitary.DateOfEnlistment = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.ArmyId = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.MilitaryUnit = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.MilitaryRankId = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.MilitaryPositionId = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.DateOfDemobilization = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.TheReason = STRING_EMPTY;
            };

            // onChangeAsAPartyMember
            $scope.onChangeAsAPartyMember = function () {
                $scope.modelPoliticsHealthMilitary.DayToParty = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.PartyPositionId = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.PlaceOfAdmissionToTheParty = STRING_EMPTY;

            };

            // onChangeAsWoundedSoldiersSickSoldiers
            $scope.onChangeAsWoundedSoldiersSickSoldiers = function () {
                $scope.modelPoliticsHealthMilitary.DateToJoinRevolution = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.RankId = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.RateOfLaborDecline = STRING_EMPTY;
                $scope.modelPoliticsHealthMilitary.EnjoyTheMode = false;
            };

        }]);

})(window.angular);
