using System;

namespace NHIGIA.Core.Domain.Entity
{
    public class ProfileEntity : EmployeeInformationEntity
    {
        public int? EmployeeId { get; set; }
        public string MobilePhone { get; set; }
        public string OfficePhone { get; set; }
        public string HomePhone { get; set; }
        public string OtherPhone { get; set; }
        public string PersonalEmail { get; set; }
        public string CompanyEmail { get; set; }
        public string OtherEmail { get; set; }
        public string Skype { get; set; }
        public string Facebook { get; set; }
        public string Domicile { get; set; }
        public int? ProvinceCityId { get; set; }
        public string PlaceBirth { get; set; }
        public int? ResidenceNationalityId { get; set; }
        public int? ResidenceProvinceCityId { get; set; }
        public int? ResidenceDistrictId { get; set; }
        public int? ResidenceWardsId { get; set; }
        public string ResidenceHouseStreetVillageNumber { get; set; }
        public string ResidenceAddress { get; set; }
        public string ResidenceHouseholdRegistrationNumber { get; set; }
        public string ResidenceHouseholdCode { get; set; }
        public bool? ResidenceIsHeadHousehold { get; set; }
        public int? CurrentNationalityId { get; set; }
        public int? CurrentProvinceCityId { get; set; }
        public int? CurrentDistrictId { get; set; }
        public int? CurrentWardsId { get; set; }
        public string CurrentHouseStreetVillageNumber { get; set; }
        public string CurrentAddress { get; set; }
        public string UrgentContactFirstAndLastName { get; set; }
        public int? UrgentContactRelationshipId { get; set; }
        public string UrgentContactMobilePhone { get; set; }
        public string UrgentContactHomePhone { get; set; }
        public string UrgentContactEmail { get; set; }
        public string UrgentContactAddress { get; set; }
        public string TimekeepingCode { get; set; }
        public int? WorkStatusId { get; set; }
        public int? DirectManagementId { get; set; }
        public int? IndirectManagementId { get; set; }
        public int? WorkLocationId { get; set; }
        public string LaborManagementBookNumber { get; set; }
        public int? ContractTypeId { get; set; }
        public DateTime? ApprenticeDay { get; set; }
        public DateTime? ProbationDay { get; set; }
        public DateTime? OfficialDate { get; set; }
        public string NumberOfDaysOff { get; set; }
        public bool? AutomaticallyIncreasesMagicAccordingToSeniority { get; set; }
        public string IncreaseLaterSpells { get; set; }
        public int? WageId { get; set; }
        public string BasicSalary { get; set; }
        public string InsurancePremiums { get; set; }
        public string StandardPublicNumber { get; set; }
        public int? StandardPublicId { get; set; }
        public string BankAccoun { get; set; }
        public int? BankId { get; set; }
        public bool? JoinTheUnion { get; set; }
        public DateTime? DateOfInsurance { get; set; }
        public string InsurancePremiumRate { get; set; }
        public string SomeSocialInsuranceBooks { get; set; }
        public string SocialInsuranceNumber { get; set; }
        public string ProvinceCodeLevel { get; set; }
        public int? ProvinceNameLevelId { get; set; }
        public string HealthInsuranceCardNumber { get; set; }
        public DateTime? HealthInsuranceExpirationDate { get; set; }
        public int? PlaceOfRegistrationForMedicalExaminationAndTreatmentId { get; set; }
        public string CodesOfMedicalExaminationAndTreatmentPlaces { get; set; }
        public string DirectManagementName { get; set; }
        public string IndirectManagementName { get; set; }
    }
}
