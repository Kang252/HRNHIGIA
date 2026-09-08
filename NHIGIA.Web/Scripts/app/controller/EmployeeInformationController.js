(function (angular) {
    "use strict";
    hrmApp.controller('EmployeeInformationController', [
        '$scope', 'EmployeeInformationService', 'ListCategoryService', 'FamilyInformationService', 'PoliticsHealthMilitaryInformationService',
        'WorkProgressInformationService', 'ContractInformationService', 'SalaryHistoryInformationService', 'BonusInformationService',
        'ProblemInformationService', 'TrainingProcessInformationService', 'DegreeInformationService', 'CertificateInformationService',
        'WorkExperienceInformationService', 'SkillInformationService', 'ReceiveInformationService', 'QuitInformationService', '$window',
        'AssetInformationService', 'SkinInformationService', 'PageInformationService', 'AttachmentInformationService', 'EvaluateService',
        'AllowanceInformationService', 'EmployeesOnBusinessTripService', 'ResignationProceduresService',
        function ($scope, EmployeeInformationService, ListCategoryService, FamilyInformationService, PoliticsHealthMilitaryInformationService,
            WorkProgressInformationService, ContractInformationService, SalaryHistoryInformationService, BonusInformationService,
            ProblemInformationService, TrainingProcessInformationService, DegreeInformationService, CertificateInformationService,
            WorkExperienceInformationService, SkillInformationService, ReceiveInformationService, QuitInformationService, $window,
            AssetInformationService, SkinInformationService, PageInformationService, AttachmentInformationService, EvaluateService,
            AllowanceInformationService, EmployeesOnBusinessTripService, ResignationProceduresService) {

            $(document).ready(function () {
                $('.employee_information').removeClass('active');
                $('.employee_information').addClass('active');
            });

            // Define
            $scope.IsSave = false;
            $scope.formErrors = {};

            $scope.model = {};
            $scope.modelFamily = {};
            $scope.modelPoliticsHealthMilitary = {};
            $scope.modelContract = {};
            $scope.modelSkin = {};

            $scope.model = {
                Image: ImageUrl
            }

            $scope.maxDate = new Date();
            $scope.ShowList = true;
            $scope.ShowInfo = false;
            $scope.HideButton = true;
            $scope.ShowButton = false;

            $scope.CheckDateOfIssueOfIdentificationCardLessIdentificationCardExpirationDate = false;
            $scope.CheckPassportDateLessPassportExpirationDate = false;
            $scope.CheckApprenticeDayLessProbationDay = false;
            $scope.CheckProbationDayLessOfficialDate = false;

            $scope.IdTemp = 0;
            $scope.WorkStatusTemp = 0;
            $scope.EmployeeTemp = STRING_EMPTY;

            $scope.MessageErrorForInput = MESSAGE_ERROR_FOR_INPUT;
            $scope.MessageErrorForSelect = MESSAGE_ERROR_FOR_SELECT;
            $scope.MessageCheckDate = "Ngày cấp phải nhỏ hơn Ngày hết hạn";
            $scope.MessageCheckApprenticeDayLessProbationDay = "Ngày tập sự phải nhỏ hơn Ngày thử việc";
            $scope.MessageCheckProbationDayLessOfficialDate = "Ngày thử việc phải nhỏ hơn Ngày chính thức";

            // BindDataToDropdownlist
            function BindDataToDropdownlist() {
                ListCategoryService.GetDataForDropdown(WorkUnit).then(function (response) {
                    $scope.WorkUnitDropdownlist = [];
                    if (response.status === 200) {
                        $scope.WorkUnitDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(JobPosition).then(function (response) {
                    $scope.JobPositionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.JobPositionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Sex).then(function (response) {
                    $scope.SexDropdownlist = [];
                    if (response.status === 200) {
                        $scope.SexDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Nation).then(function (response) {
                    $scope.NationDropdownlist = [];
                    if (response.status === 200) {
                        $scope.NationDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Religion).then(function (response) {
                    $scope.ReligionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ReligionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetAllNationality().then(function (response) {
                    $scope.NationalityDropdownlist = [];
                    if (response.status === 200) {
                        $scope.NationalityDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(EducationalLevel).then(function (response) {
                    $scope.EducationalLevelDropdownlist = [];
                    if (response.status === 200) {
                        $scope.EducationalLevelDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(MaritalStatus).then(function (response) {
                    $scope.MaritalStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.MaritalStatusDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(DegreeTraining).then(function (response) {
                    $scope.DegreeTrainingDropdownlist = [];
                    if (response.status === 200) {
                        $scope.DegreeTrainingDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(FamilyMember).then(function (response) {
                    $scope.FamilyMemberDropdownlist = [];
                    if (response.status === 200) {
                        $scope.FamilyMemberDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(TrainingPlaces).then(function (response) {
                    $scope.TrainingPlacesDropdownlist = [];
                    if (response.status === 200) {
                        $scope.TrainingPlacesDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(IngredientsThemselves).then(function (response) {
                    $scope.IngredientsThemselvesDropdownlist = [];
                    if (response.status === 200) {
                        $scope.IngredientsThemselvesDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Faculty).then(function (response) {
                    $scope.FacultyDropdownlist = [];
                    if (response.status === 200) {
                        $scope.FacultyDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Specialized).then(function (response) {
                    $scope.SpecializedDropdownlist = [];
                    if (response.status === 200) {
                        $scope.SpecializedDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Classification).then(function (response) {
                    $scope.ClassificationDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ClassificationDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetAllProvinceCity(0).then(function (response) {
                    $scope.ProvinceCityDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ProvinceCityDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Relationship).then(function (response) {
                    $scope.RelationshipDropdownlist = [];
                    if (response.status === 200) {
                        $scope.RelationshipDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetStatusForDropdown(WorkStatus).then(function (response) {
                    $scope.WorkStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.WorkStatusDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(WorkLocation).then(function (response) {
                    $scope.WorkLocationDropdownlist = [];
                    if (response.status === 200) {
                        $scope.WorkLocationDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(ContractType).then(function (response) {
                    $scope.ContractTypeDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ContractTypeDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Wage).then(function (response) {
                    $scope.WageDropdownlist = [];
                    if (response.status === 200) {
                        $scope.WageDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(StandardPublic).then(function (response) {
                    $scope.StandardPublicDropdownlist = [];
                    if (response.status === 200) {
                        $scope.StandardPublicDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Bank).then(function (response) {
                    $scope.BankDropdownlist = [];
                    if (response.status === 200) {
                        $scope.BankDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(PlaceOfRegistrationForMedicalExaminationAndTreatment).then(function (response) {
                    $scope.PlaceOfRegistrationForMedicalExaminationAndTreatmentDropdownlist = [];
                    if (response.status === 200) {
                        $scope.PlaceOfRegistrationForMedicalExaminationAndTreatmentDropdownlist = response.data;
                    }
                });
            };

            BindDataToDropdownlist();

            // BindDataToDropdownlistForPoliticsHealthMilitaryInformation
            function BindDataToDropdownlistForPoliticsHealthMilitaryInformation() {
                ListCategoryService.GetDataForDropdown(GroupPosition).then(function (response) {
                    $scope.GroupPositionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.GroupPositionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Army).then(function (response) {
                    $scope.ArmyDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ArmyDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(MilitaryRank).then(function (response) {
                    $scope.MilitaryRankDropdownlist = [];
                    if (response.status === 200) {
                        $scope.MilitaryRankDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(MilitaryPosition).then(function (response) {
                    $scope.MilitaryPositionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.MilitaryPositionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(PartyPosition).then(function (response) {
                    $scope.PartyPositionDropdownlist = [];
                    if (response.status === 200) {
                        $scope.PartyPositionDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(Rank).then(function (response) {
                    $scope.RankDropdownlist = [];
                    if (response.status === 200) {
                        $scope.RankDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(BloodGroup).then(function (response) {
                    $scope.BloodGroupDropdownlist = [];
                    if (response.status === 200) {
                        $scope.BloodGroupDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForContractInformation
            function BindDataToDropdownlistForContractInformation() {
                ListCategoryService.GetDataForDropdown(ContractTerm).then(function (response) {
                    $scope.ContractTermDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ContractTermDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(TheFormOfWork).then(function (response) {
                    $scope.TheFormOfWorkDropdownlist = [];
                    if (response.status === 200) {
                        $scope.TheFormOfWorkDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForSkinInformation
            function BindDataToDropdownlistForSkinInformation() {
                ListCategoryService.GetDataForDropdown(FontSize).then(function (response) {
                    $scope.FontSizeDropdownlist = [];
                    if (response.status === 200) {
                        $scope.FontSizeDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(NumberSize).then(function (response) {
                    $scope.NumberSizeDropdownlist = [];
                    if (response.status === 200) {
                        $scope.NumberSizeDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForSkillInformation
            function BindDataToDropdownlistForSkillInformation() {
                ListCategoryService.GetDataForDropdown(SkillGroup).then(function (response) {
                    $scope.SkillGroupDropdownlist = [];
                    if (response.status === 200) {
                        $scope.SkillGroupDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(SkillLevel).then(function (response) {
                    $scope.SkillLevelDropdownlist = [];
                    if (response.status === 200) {
                        $scope.SkillLevelDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForReceiveInformation
            function BindDataToDropdownlistForReceiveInformation() {
                ListCategoryService.GetDataForDropdown(ProcedureGroupReceive).then(function (response) {
                    $scope.ProcedureGroupReceiveDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ProcedureGroupReceiveDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForQuitInformation
            function BindDataToDropdownlistForQuitInformation() {
                ListCategoryService.GetDataForDropdown(ProcedureGroupQuit).then(function (response) {
                    $scope.ProcedureGroupQuitDropdownlist = [];
                    if (response.status === 200) {
                        $scope.ProcedureGroupQuitDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForAssetInformation
            function BindDataToDropdownlistForAssetInformation() {
                ListCategoryService.GetDataForDropdown(AssetType).then(function (response) {
                    $scope.AssetTypeDropdownlist = [];
                    if (response.status === 200) {
                        $scope.AssetTypeDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(AssetStatus).then(function (response) {
                    $scope.AssetStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.AssetStatusDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForAllowanceInformation
            function BindDataToDropdownlistForAllowanceInformation() {
                ListCategoryService.GetDataForDropdown(AllowanceType).then(function (response) {
                    $scope.AllowanceTypeDropdownlist = [];
                    if (response.status === 200) {
                        $scope.AllowanceTypeDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForBonusInformation
            function BindDataToDropdownlistForBonusInformation() {
                ListCategoryService.GetDataForDropdown(RewardReason).then(function (response) {
                    $scope.RewardReasonDropdownlist = [];
                    if (response.status === 200) {
                        $scope.RewardReasonDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(CommendationForm).then(function (response) {
                    $scope.CommendationFormDropdownlist = [];
                    if (response.status === 200) {
                        $scope.CommendationFormDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(StatusBonus).then(function (response) {
                    $scope.StatusBonusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.StatusBonusDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForProblemInformation
            function BindDataToDropdownlistForProblemInformation() {
                ListCategoryService.GetDataForDropdown(TypeOfIncident).then(function (response) {
                    $scope.TypeOfIncidentDropdownlist = [];
                    if (response.status === 200) {
                        $scope.TypeOfIncidentDropdownlist = response.data;
                    }
                });
                ListCategoryService.GetDataForDropdown(CompensationStatus).then(function (response) {
                    $scope.CompensationStatusDropdownlist = [];
                    if (response.status === 200) {
                        $scope.CompensationStatusDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForDegreeInformation
            function BindDataToDropdownlistForDegreeInformation() {
                ListCategoryService.GetDataForDropdown(FormsOfTraining).then(function (response) {
                    $scope.FormsOfTrainingDropdownlist = [];
                    if (response.status === 200) {
                        $scope.FormsOfTrainingDropdownlist = response.data;
                    }
                });
            };

            // BindDataToDropdownlistForCertificateInformation
            function BindDataToDropdownlistForCertificateInformation() {
                ListCategoryService.GetDataForDropdown(CertificateGroup).then(function (response) {
                    $scope.CertificateGroupDropdownlist = [];
                    if (response.status === 200) {
                        $scope.CertificateGroupDropdownlist = response.data;
                    }
                });
            };

            // onChange
            function onChange(arg) {
                $scope.IdTemp = arg.sender.dataItem(arg.sender.select()).Id;

                // Load data for Nationality, ProvinceCity, District, Wards
                EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                    if (response.status === 200 && response.data.Success === true) {

                        $scope.WorkStatusTemp = response.data.Data.WorkStatusId;
                        $scope.EmployeeTemp = response.data.Data.EmployeeCode + ' - ' + response.data.Data.EmployeeName;

                        $scope.ShowHideButton();

                        // Load data for ProvinceCity
                        if (response.data.Data.ResidenceNationalityId) {
                            ListCategoryService.GetAllProvinceCity(response.data.Data.ResidenceNationalityId).then(function (response) {
                                if (response.status === 200) {
                                    $scope.ResidenceProvinceCityDropdownlist = response.data;
                                }
                            });
                        }
                        else {
                            $scope.ResidenceProvinceCityDropdownlist = [];
                        }
                        if (response.data.Data.CurrentNationalityId) {
                            ListCategoryService.GetAllProvinceCity(response.data.Data.CurrentNationalityId).then(function (response) {
                                if (response.status === 200) {
                                    $scope.CurrentProvinceCityDropdownlist = response.data;
                                }
                            });
                        } else {
                            $scope.CurrentProvinceCityDropdownlist = [];
                        }

                        // Load data for District
                        if (response.data.Data.ResidenceProvinceCityId) {
                            ListCategoryService.GetAllDistrict(response.data.Data.ResidenceProvinceCityId).then(function (response) {
                                if (response.status === 200) {
                                    $scope.ResidenceDistrictDropdownlist = response.data;
                                }
                            });
                        } else {
                            $scope.ResidenceDistrictDropdownlist = [];
                        }
                        if (response.data.Data.CurrentProvinceCityId) {
                            ListCategoryService.GetAllDistrict(response.data.Data.CurrentProvinceCityId).then(function (response) {
                                if (response.status === 200) {
                                    $scope.CurrentDistrictDropdownlist = response.data;
                                }
                            });
                        } else {
                            $scope.CurrentDistrictDropdownlist = [];
                        }

                        // Load data for Wards
                        if (response.data.Data.ResidenceDistrictId) {
                            ListCategoryService.GetAllWards(response.data.Data.ResidenceDistrictId).then(function (response) {
                                if (response.status === 200) {
                                    $scope.ResidenceWardsDropdownlist = response.data;
                                }
                            });
                        } else {
                            $scope.ResidenceWardsDropdownlist = [];
                        }
                        if (response.data.Data.CurrentDistrictId) {
                            ListCategoryService.GetAllWards(response.data.Data.CurrentDistrictId).then(function (response) {
                                if (response.status === 200) {
                                    $scope.CurrentWardsDropdownlist = response.data;
                                }
                            });
                        } else {
                            $scope.CurrentWardsDropdownlist = [];
                        }
                    }
                });

                $("#ButtonEdit").prop("disabled", false);
                $("#ButtonDelete").prop("disabled", false);
                $("#ButtonResignationProcedures").prop("disabled", false);
            }

            // onDisable
            function onDisable() {
                $("#ButtonEdit").prop("disabled", true);
                $("#ButtonDelete").prop("disabled", true);
                $("#ButtonResignationProcedures").prop("disabled", true);
                $scope.IdTemp = 0;
            }

            // Designer gird
            $scope.Section = {
                dataSource: EmployeeInformationService.GetAllEmployeeInformation(),
                resizeable: true,
                autoBind: true,
                sortable: {
                    mode: SINGLE,
                    allowUnsort: true
                },
                change: onChange,
                selectable: true,
                noRecords: true,
                messages: {
                    noRecords: NORECORDS
                },
                filterable: {
                    extra: false,
                    operators: {
                        string: {
                            contains: CONTENT_FILTER
                        }
                    },
                    messages: {
                        info: STRING_EMPTY,
                        filter: FILTER,
                        clear: CLEAR,
                        search: STRING_EMPTY,
                        selectedItemsFormat: SELECTEDITEMSFORMAT
                    },
                    mode: ROW
                },
                filter: function (e) {
                    onDisable();
                },
                sort: function (e) {
                    onDisable();
                },
                dataBinding: function (e) {
                    onDisable();
                },
                scrollable: true,
                pageable: {
                    refresh: false,
                    input: false,
                    pageSizes: [10, 50, 100],
                    buttonCount: 5,
                    messages: {
                        itemsPerPage: ITEMSPERPAGE,
                        display: DISPLAY,
                        empty: EMPTY
                    },
                    change: function (e) {
                        onDisable();
                    },
                },
                columns: [
                    {
                        field: "EmployeeCode", title: "Mã nhân viên", width: "150px", locked: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "EmployeeName", title: "Họ và tên", width: "150px", locked: true,
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "SexName", title: "Giới tính", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "DateOfBirth", title: "Ngày sinh", width: "150px",
                        template: "<span>#= (DateOfBirth == null) ? '' : kendo.toString(kendo.parseDate(DateOfBirth, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "JobPositionName", title: "Vị trí công việc", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "WorkUnitName", title: "Đơn vị công tác", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "WorkStatusName", title: "Trạng thái", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "DegreeTrainingName", title: "Trình độ đào tạo", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "TrainingPlacesName", title: "Nơi đào tạo", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "SpecializedName", title: "Chuyên ngành", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "ProbationDay", title: "Ngày thử việc", width: "150px",
                        template: "<span>#= (ProbationDay == null) ? '' : kendo.toString(kendo.parseDate(ProbationDay, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "OfficialDate", title: "Ngày chính thức", width: "150px",
                        template: "<span>#= (OfficialDate == null) ? '' : kendo.toString(kendo.parseDate(OfficialDate, 'dd/MM/yyyy'), 'dd/MM/yyyy') # ",
                        format: "{0:dd/MM/yyyy}",
                        parseFomats: "{0:dd/MM/yyyy}",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    },
                    {
                        field: "ContractTypeName", title: "Loại hợp đồng", width: "150px",
                        filterable: {
                            cell: {
                                showOperators: false,
                                delay: 1500000000
                            }
                        }
                    }
                ]
            };

            // ShowPopup
            $scope.ShowPopup = function (e) {
                $scope.IsSave = false;
                var valueClick = e;

                $scope.model = {
                    //EmployeeInformation
                    Id: 0,
                    Image: ImageUrl,
                    EmployeeCode: STRING_EMPTY,
                    EmployeeName: STRING_EMPTY,
                    SexId: STRING_EMPTY,
                    DateOfBirth: STRING_EMPTY,
                    PersonalTaxCode: STRING_EMPTY,
                    WorkUnitId: STRING_EMPTY,
                    JobPositionId: STRING_EMPTY,
                    NationId: STRING_EMPTY,
                    ReligionId: STRING_EMPTY,
                    NationalityId: STRING_EMPTY,
                    IdentificationCardNumber: STRING_EMPTY,
                    DateOfIssueOfIdentificationCard: STRING_EMPTY,
                    PlaceOfIssueOfIdCard: STRING_EMPTY,
                    IdentificationCardExpirationDate: STRING_EMPTY,
                    PassportNumber: STRING_EMPTY,
                    PassportDate: STRING_EMPTY,
                    PlaceOfIssueOfPassport: STRING_EMPTY,
                    PassportExpirationDate: STRING_EMPTY,
                    EducationalLevelId: STRING_EMPTY,
                    DegreeTrainingId: STRING_EMPTY,
                    TrainingPlacesId: STRING_EMPTY,
                    FacultyId: STRING_EMPTY,
                    SpecializedId: STRING_EMPTY,
                    GraduationYear: STRING_EMPTY,
                    ClassificationId: STRING_EMPTY,
                    MaritalStatusId: STRING_EMPTY,
                    FamilyMemberId: STRING_EMPTY,
                    IngredientsThemselvesId: STRING_EMPTY,

                    //JobInformation
                    EmployeeId: STRING_EMPTY,
                    TimekeepingCode: STRING_EMPTY,
                    WorkStatusId: 1, // Đang làm việc
                    DirectManagementId: STRING_EMPTY,
                    IndirectManagementId: STRING_EMPTY,
                    WorkLocationId: STRING_EMPTY,
                    LaborManagementBookNumber: STRING_EMPTY,
                    ContractTypeId: STRING_EMPTY,
                    ApprenticeDay: STRING_EMPTY,
                    ProbationDay: STRING_EMPTY,
                    OfficialDate: STRING_EMPTY,
                    NumberOfDaysOff: STRING_EMPTY,
                    AutomaticallyIncreasesMagicAccordingToSeniority: true,
                    IncreaseLaterSpells: "5 năm",
                    WageId: STRING_EMPTY,
                    BasicSalary: 0,
                    InsurancePremiums: 0,
                    StandardPublicNumber: STRING_EMPTY,
                    StandardPublicId: STRING_EMPTY,
                    BankAccoun: STRING_EMPTY,
                    BankId: STRING_EMPTY,
                    JoinTheUnion: false,
                    DateOfInsurance: STRING_EMPTY,
                    InsurancePremiumRate: STRING_EMPTY,
                    SomeSocialInsuranceBooks: STRING_EMPTY,
                    SocialInsuranceNumber: STRING_EMPTY,
                    ProvinceCodeLevel: STRING_EMPTY,
                    ProvinceNameLevelId: STRING_EMPTY,
                    HealthInsuranceCardNumber: STRING_EMPTY,
                    HealthInsuranceExpirationDate: STRING_EMPTY,
                    PlaceOfRegistrationForMedicalExaminationAndTreatmentId: STRING_EMPTY,
                    CodesOfMedicalExaminationAndTreatmentPlaces: STRING_EMPTY,
                    DirectManagementAutoComplete: STRING_EMPTY,
                    DirectManagementNameTemp: STRING_EMPTY,
                    IndirectManagementAutoComplete: STRING_EMPTY,
                    IndirectManagementNameTemp: STRING_EMPTY,

                    //ContactInformation
                    MobilePhone: STRING_EMPTY,
                    OfficePhone: STRING_EMPTY,
                    HomePhone: STRING_EMPTY,
                    OtherPhone: STRING_EMPTY,
                    PersonalEmail: STRING_EMPTY,
                    CompanyEmail: STRING_EMPTY,
                    OtherEmail: STRING_EMPTY,
                    Skype: STRING_EMPTY,
                    Facebook: STRING_EMPTY,
                    Domicile: STRING_EMPTY,
                    ProvinceCityId: STRING_EMPTY,
                    PlaceBirth: STRING_EMPTY,
                    ResidenceNationalityId: STRING_EMPTY,
                    ResidenceProvinceCityId: STRING_EMPTY,
                    ResidenceDistrictId: STRING_EMPTY,
                    ResidenceWardsId: STRING_EMPTY,
                    ResidenceHouseStreetVillageNumber: STRING_EMPTY,
                    ResidenceAddress: STRING_EMPTY,
                    ResidenceHouseholdRegistrationNumber: STRING_EMPTY,
                    ResidenceHouseholdCode: STRING_EMPTY,
                    ResidenceIsHeadHousehold: false,
                    CurrentNationalityId: STRING_EMPTY,
                    CurrentProvinceCityId: STRING_EMPTY,
                    CurrentDistrictId: STRING_EMPTY,
                    CurrentWardsId: STRING_EMPTY,
                    CurrentHouseStreetVillageNumber: STRING_EMPTY,
                    CurrentAddress: STRING_EMPTY,
                    UrgentContactFirstAndLastName: STRING_EMPTY,
                    UrgentContactRelationshipId: STRING_EMPTY,
                    UrgentContactMobilePhone: STRING_EMPTY,
                    UrgentContactHomePhone: STRING_EMPTY,
                    UrgentContactEmail: STRING_EMPTY,
                    UrgentContactAddress: STRING_EMPTY
                };

                switch (valueClick) {
                    case "ADD":
                        loadingProfilePage();
                        $scope.ShowList = false;
                        $scope.ShowInfo = true;
                        stopLoadingProfilePage();
                        break;
                    case "EDIT":
                        loadingProfilePage();
                        if ($scope.IdTemp === 0) {
                            EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                                if (response.data.status === 404) {
                                    bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                                    $scope.IdTemp = 0;
                                    stopLoadingProfilePage();
                                    return;
                                }
                            });
                        } else {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                            EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                                if (response.status === 200 && response.data.Success === true) {

                                    // check show hide button save by workstatus
                                    $scope.ShowHideButton();

                                    $scope.model.Id = $scope.IdTemp;
                                    $scope.model.Image = response.data.Data.Image;
                                    $scope.model.EmployeeCode = response.data.Data.EmployeeCode;
                                    $scope.model.EmployeeName = response.data.Data.EmployeeName;
                                    $scope.model.SexId = response.data.Data.SexId;
                                    $scope.model.DateOfBirth = kendo.parseDate(response.data.Data.DateOfBirth, DATE_FORMAT);
                                    $scope.model.PersonalTaxCode = response.data.Data.PersonalTaxCode;
                                    $scope.model.WorkUnitId = response.data.Data.WorkUnitId;
                                    $scope.model.JobPositionId = response.data.Data.JobPositionId;
                                    $scope.model.NationId = response.data.Data.NationId;
                                    $scope.model.ReligionId = response.data.Data.ReligionId;
                                    $scope.model.NationalityId = response.data.Data.NationalityId;
                                    $scope.model.IdentificationCardNumber = response.data.Data.IdentificationCardNumber;
                                    $scope.model.DateOfIssueOfIdentificationCard = kendo.parseDate(response.data.Data.DateOfIssueOfIdentificationCard, DATE_FORMAT);
                                    $scope.model.PlaceOfIssueOfIdCard = response.data.Data.PlaceOfIssueOfIdCard;
                                    $scope.model.IdentificationCardExpirationDate = kendo.parseDate(response.data.Data.IdentificationCardExpirationDate, DATE_FORMAT);
                                    $scope.model.PassportNumber = response.data.Data.PassportNumber;
                                    $scope.model.PassportDate = kendo.parseDate(response.data.Data.PassportDate, DATE_FORMAT);
                                    $scope.model.PlaceOfIssueOfPassport = response.data.Data.PlaceOfIssueOfPassport;
                                    $scope.model.PassportExpirationDate = kendo.parseDate(response.data.Data.PassportExpirationDate, DATE_FORMAT);
                                    $scope.model.EducationalLevelId = response.data.Data.EducationalLevelId;
                                    $scope.model.DegreeTrainingId = response.data.Data.DegreeTrainingId;
                                    $scope.model.TrainingPlacesId = response.data.Data.TrainingPlacesId;
                                    $scope.model.FacultyId = response.data.Data.FacultyId;
                                    $scope.model.SpecializedId = response.data.Data.SpecializedId;
                                    $scope.model.GraduationYear = response.data.Data.GraduationYear;
                                    $scope.model.ClassificationId = response.data.Data.ClassificationId;
                                    $scope.model.MaritalStatusId = response.data.Data.MaritalStatusId;
                                    $scope.model.FamilyMemberId = response.data.Data.FamilyMemberId;
                                    $scope.model.IngredientsThemselvesId = response.data.Data.IngredientsThemselvesId;
                                    $scope.model.MobilePhone = response.data.Data.MobilePhone;
                                    $scope.model.OfficePhone = response.data.Data.OfficePhone;
                                    $scope.model.HomePhone = response.data.Data.HomePhone;
                                    $scope.model.OtherPhone = response.data.Data.OtherPhone;
                                    $scope.model.PersonalEmail = response.data.Data.PersonalEmail;
                                    $scope.model.CompanyEmail = response.data.Data.CompanyEmail;
                                    $scope.model.OtherEmail = response.data.Data.OtherEmail;
                                    $scope.model.Skype = response.data.Data.Skype;
                                    $scope.model.Facebook = response.data.Data.Facebook;
                                    $scope.model.Domicile = response.data.Data.Domicile;
                                    $scope.model.ProvinceCityId = response.data.Data.ProvinceCityId;
                                    $scope.model.PlaceBirth = response.data.Data.PlaceBirth;
                                    $scope.model.ResidenceNationalityId = response.data.Data.ResidenceNationalityId;
                                    $scope.model.ResidenceProvinceCityId = response.data.Data.ResidenceProvinceCityId;
                                    $scope.model.ResidenceDistrictId = response.data.Data.ResidenceDistrictId;
                                    $scope.model.ResidenceWardsId = response.data.Data.ResidenceWardsId;
                                    $scope.model.ResidenceHouseStreetVillageNumber = response.data.Data.ResidenceHouseStreetVillageNumber;
                                    $scope.model.ResidenceAddress = response.data.Data.ResidenceAddress;
                                    $scope.model.ResidenceHouseholdRegistrationNumber = response.data.Data.ResidenceHouseholdRegistrationNumber;
                                    $scope.model.ResidenceHouseholdCode = response.data.Data.ResidenceHouseholdCode;
                                    $scope.model.ResidenceIsHeadHousehold = response.data.Data.ResidenceIsHeadHousehold;
                                    $scope.model.CurrentNationalityId = response.data.Data.CurrentNationalityId;
                                    $scope.model.CurrentProvinceCityId = response.data.Data.CurrentProvinceCityId;
                                    $scope.model.CurrentDistrictId = response.data.Data.CurrentDistrictId;
                                    $scope.model.CurrentWardsId = response.data.Data.CurrentWardsId;
                                    $scope.model.CurrentHouseStreetVillageNumber = response.data.Data.CurrentHouseStreetVillageNumber;
                                    $scope.model.CurrentAddress = response.data.Data.CurrentAddress;
                                    $scope.model.UrgentContactFirstAndLastName = response.data.Data.UrgentContactFirstAndLastName;
                                    $scope.model.UrgentContactRelationshipId = response.data.Data.UrgentContactRelationshipId;
                                    $scope.model.UrgentContactMobilePhone = response.data.Data.UrgentContactMobilePhone;
                                    $scope.model.UrgentContactHomePhone = response.data.Data.UrgentContactHomePhone;
                                    $scope.model.UrgentContactEmail = response.data.Data.UrgentContactEmail;
                                    $scope.model.UrgentContactAddress = response.data.Data.UrgentContactAddress;
                                    $scope.model.TimekeepingCode = response.data.Data.TimekeepingCode;
                                    $scope.model.WorkStatusId = response.data.Data.WorkStatusId;
                                    $scope.model.DirectManagementId = response.data.Data.DirectManagementId;
                                    $scope.model.IndirectManagementId = response.data.Data.IndirectManagementId;
                                    $scope.model.WorkLocationId = response.data.Data.WorkLocationId;
                                    $scope.model.LaborManagementBookNumber = response.data.Data.LaborManagementBookNumber;
                                    $scope.model.ContractTypeId = response.data.Data.ContractTypeId;
                                    $scope.model.ApprenticeDay = kendo.parseDate(response.data.Data.ApprenticeDay, DATE_FORMAT);
                                    $scope.model.ProbationDay = kendo.parseDate(response.data.Data.ProbationDay, DATE_FORMAT);
                                    $scope.model.OfficialDate = kendo.parseDate(response.data.Data.OfficialDate, DATE_FORMAT);
                                    $scope.model.NumberOfDaysOff = response.data.Data.NumberOfDaysOff;
                                    $scope.model.AutomaticallyIncreasesMagicAccordingToSeniority = response.data.Data.AutomaticallyIncreasesMagicAccordingToSeniority;
                                    $scope.model.IncreaseLaterSpells = response.data.Data.IncreaseLaterSpells;
                                    $scope.model.WageId = response.data.Data.WageId;
                                    $scope.model.BasicSalary = response.data.Data.BasicSalary;
                                    $scope.model.InsurancePremiums = response.data.Data.InsurancePremiums;
                                    $scope.model.StandardPublicNumber = response.data.Data.StandardPublicNumber;
                                    $scope.model.StandardPublicId = response.data.Data.StandardPublicId;
                                    $scope.model.BankAccoun = response.data.Data.BankAccoun;
                                    $scope.model.BankId = response.data.Data.BankId;
                                    $scope.model.JoinTheUnion = response.data.Data.JoinTheUnion;
                                    $scope.model.DateOfInsurance = kendo.parseDate(response.data.Data.DateOfInsurance, DATE_FORMAT);
                                    $scope.model.InsurancePremiumRate = response.data.Data.InsurancePremiumRate;
                                    $scope.model.SomeSocialInsuranceBooks = response.data.Data.SomeSocialInsuranceBooks;
                                    $scope.model.SocialInsuranceNumber = response.data.Data.SocialInsuranceNumber;
                                    $scope.model.ProvinceCodeLevel = response.data.Data.ProvinceCodeLevel;
                                    $scope.model.ProvinceNameLevelId = response.data.Data.ProvinceNameLevelId;
                                    $scope.model.HealthInsuranceCardNumber = response.data.Data.HealthInsuranceCardNumber;
                                    $scope.model.HealthInsuranceExpirationDate = kendo.parseDate(response.data.Data.HealthInsuranceExpirationDate, DATE_FORMAT);
                                    $scope.model.PlaceOfRegistrationForMedicalExaminationAndTreatmentId = response.data.Data.PlaceOfRegistrationForMedicalExaminationAndTreatmentId;
                                    $scope.model.CodesOfMedicalExaminationAndTreatmentPlaces = response.data.Data.CodesOfMedicalExaminationAndTreatmentPlaces;
                                    $scope.model.DirectManagementAutoComplete = response.data.Data.DirectManagementName;
                                    $scope.model.IndirectManagementAutoComplete = response.data.Data.IndirectManagementName;
                                    $scope.model.DirectManagementNameTemp = response.data.Data.DirectManagementName;
                                    $scope.model.IndirectManagementNameTemp = response.data.Data.IndirectManagementName;
                                    stopLoadingProfilePage();
                                }
                            });
                        }
                        break;
                    case "CANCEL":
                        loadingProfilePage();
                        $scope.ShowInfo = false;
                        $window.location.reload();
                        stopLoadingProfilePage();
                        break;
                    case "REFRESH":
                        loadingProfilePage();
                        $scope.WorkStatusTemp = 0;
                        $scope.ShowHideButton();
                        RefreshKendoGrid("Table");
                        stopLoadingProfilePage();
                        break;
                    case "RESIGNATION_PROCEDURES":
                        loadingProfilePage();
                        $scope.modelResignationProcedures = {
                            EmployeeId: STRING_EMPTY,
                            ExpectedResignationDate: new Date().getDate() + "/" + (new Date().getMonth() + 1) + "/" + new Date().getFullYear()
                        };

                        // 2 = Đang làm thủ tục thôi việc, 3 = Nghỉ hưu, 8 = Nghỉ việc
                        if ($scope.WorkStatusTemp === 2) {
                            stopLoadingProfilePage();
                            bootbox.alert("<span style='color:green; text-align:justify;'>Nhân viên " + $scope.EmployeeTemp + " đang làm thủ tục thôi việc</span>");
                        } else if ($scope.WorkStatusTemp === 3) {
                            stopLoadingProfilePage();
                            bootbox.alert("<span style='color:green; text-align:justify;'>Nhân viên " + $scope.EmployeeTemp + " đã nghỉ hưu</span>");
                        } else if ($scope.WorkStatusTemp === 8) {
                            stopLoadingProfilePage();
                            bootbox.alert("<span style='color:green; text-align:justify;'>Nhân viên " + $scope.EmployeeTemp + " đã nghỉ việc</span>");
                        } else {
                            $("#btnSaveResignationProcedures").prop("disabled", false);
                            $("#btnCloseResignationProcedures").prop("disabled", false);
                            var windowResignationProcedures = $("#KenWindownResignationProcedures").kendoWindow({
                                actions: ["CloseResignationProcedures"],
                                draggable: true,
                                modal: true,
                                pinned: false,
                                position: {
                                    top: 15
                                },
                                resizable: false,
                                width: "50%"
                            }).data('kendoWindow');

                            if ($scope.IdTemp === 0) {
                                bootbox.alert("<span style='color:red; text-align:justify;'>" + CANNOT_FIND_ANY_WITH_GIVEN_ID + "</span>");
                                stopLoadingProfilePage();
                            } else {
                                stopLoadingProfilePage();
                                EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                                    if (response.status === 200 && response.data.Success === true) {
                                        $scope.modelResignationProcedures.WorkStatusId = response.data.Data.WorkStatusId;
                                    }
                                });
                                windowResignationProcedures.title("Làm thủ tục thôi việc");
                                windowResignationProcedures.open();
                                windowResignationProcedures.center();
                                stopLoadingProfilePage();
                            }
                        }
                        break;
                    default:
                        break;
                }
            };

            // Upsite
            $scope.Upsite = function () {
                $window.scrollTo(0, 0);
            };

            // ChoonseImage
            $scope.ChoonseImage = function () {
                var finder = new CKFinder();
                finder.selectActionFunction = function (fileUrl) {
                    $scope.model.Image = fileUrl;
                    $scope.$apply();
                };
                finder.popup();
            };

            // onSelectResidenceNationality
            $scope.onSelectResidenceNationality = function (e) {

                $scope.ResidenceProvinceCityDropdownlist = [];
                $scope.ResidenceDistrictDropdownlist = [];
                $scope.ResidenceWardsDropdownlist = [];

                // Set value string empty ResidenceProvinceCityId
                $("#ResidenceProvinceCityId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#ResidenceProvinceCityId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#ResidenceProvinceCityId").data("kendoDropDownList").trigger("change");
                $scope.model.ResidenceProvinceCityId = STRING_EMPTY;

                // Set value string empty ResidenceDistrictId
                $("#ResidenceDistrictId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#ResidenceDistrictId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#ResidenceDistrictId").data("kendoDropDownList").trigger("change");
                $scope.model.ResidenceDistrictId = STRING_EMPTY;

                // Set value string empty ResidenceWardsId
                $("#ResidenceWardsId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#ResidenceWardsId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#ResidenceWardsId").data("kendoDropDownList").trigger("change");
                $scope.model.ResidenceWardsId = STRING_EMPTY;

                //handle value ResidenceAddress
                $scope.model.ResidenceAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    ListCategoryService.GetAllProvinceCity(e.dataItem.Id).then(function (response) {
                        if (response.status === 200) {
                            $scope.ResidenceProvinceCityDropdownlist = response.data;

                            //handle value ResidenceAddress if e.dataItem.Id !== STRING_EMPTY
                            if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                                $scope.model.ResidenceAddress = $("#ResidenceNationalityId").data('kendoDropDownList').text();
                            } else {
                                $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                            }
                        }
                    });
                } else {
                    //handle value ResidenceAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#ResidenceHouseStreetVillageNumber").val() !== STRING_EMPTY) {
                        $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val();
                    }
                }
            };

            // onSelectCurrentNationality
            $scope.onSelectCurrentNationality = function (e) {

                $scope.CurrentProvinceCityDropdownlist = [];
                $scope.CurrentDistrictDropdownlist = [];
                $scope.CurrentWardsDropdownlist = [];

                // Set value string empty CurrentProvinceCityId
                $("#CurrentProvinceCityId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#CurrentProvinceCityId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#CurrentProvinceCityId").data("kendoDropDownList").trigger("change");
                $scope.model.CurrentProvinceCityId = STRING_EMPTY;

                // Set value string empty CurrentDistrictId
                $("#CurrentDistrictId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#CurrentDistrictId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#CurrentDistrictId").data("kendoDropDownList").trigger("change");
                $scope.model.CurrentDistrictId = STRING_EMPTY;

                // Set value string empty CurrentWardsId
                $("#CurrentWardsId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#CurrentWardsId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#CurrentWardsId").data("kendoDropDownList").trigger("change");
                $scope.model.CurrentWardsId = STRING_EMPTY;

                //handle value CurrentAddress
                $scope.model.CurrentAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    ListCategoryService.GetAllProvinceCity(e.dataItem.Id).then(function (response) {
                        if (response.status === 200) {
                            $scope.CurrentProvinceCityDropdownlist = response.data;

                            //handle value CurrentAddress if e.dataItem.Id !== STRING_EMPTY
                            if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                                $scope.model.CurrentAddress = $("#CurrentNationalityId").data('kendoDropDownList').text();
                            } else {
                                $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                            }
                        }
                    });
                } else {
                    //handle value CurrentAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#CurrentHouseStreetVillageNumber").val() !== STRING_EMPTY) {
                        $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val();
                    }
                }
            };

            // onSelectResidenceProvinceCity
            $scope.onSelectResidenceProvinceCity = function (e) {

                $scope.ResidenceDistrictDropdownlist = [];
                $scope.ResidenceWardsDropdownlist = [];

                // Set value string empty ResidenceDistrictId
                $("#ResidenceDistrictId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#ResidenceDistrictId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#ResidenceDistrictId").data("kendoDropDownList").trigger("change");
                $scope.model.ResidenceDistrictId = STRING_EMPTY;

                // Set value string empty ResidenceWardsId
                $("#ResidenceWardsId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#ResidenceWardsId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#ResidenceWardsId").data("kendoDropDownList").trigger("change");
                $scope.model.ResidenceWardsId = STRING_EMPTY;

                //handle value ResidenceAddress
                $scope.model.ResidenceAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    ListCategoryService.GetAllDistrict(e.dataItem.Id).then(function (response) {
                        if (response.status === 200) {
                            $scope.ResidenceDistrictDropdownlist = response.data;

                            //handle value ResidenceAddress if e.dataItem.Id !== STRING_EMPTY
                            if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                                $scope.model.ResidenceAddress = $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                            } else {
                                $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                            }
                        }
                    });
                } else {
                    //handle value ResidenceAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.ResidenceAddress = $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onSelectCurrentProvinceCity
            $scope.onSelectCurrentProvinceCity = function (e) {

                $scope.CurrentDistrictDropdownlist = [];
                $scope.CurrentWardsDropdownlist = [];

                // Set value string empty CurrentDistrictId
                $("#CurrentDistrictId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#CurrentDistrictId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#CurrentDistrictId").data("kendoDropDownList").trigger("change");
                $scope.model.CurrentDistrictId = STRING_EMPTY;

                // Set value string empty CurrentWardsId
                $("#CurrentWardsId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#CurrentWardsId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#CurrentWardsId").data("kendoDropDownList").trigger("change");
                $scope.model.CurrentWardsId = STRING_EMPTY;

                //handle value CurrentAddress
                $scope.model.CurrentAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    ListCategoryService.GetAllDistrict(e.dataItem.Id).then(function (response) {
                        if (response.status === 200) {
                            $scope.CurrentDistrictDropdownlist = response.data;

                            //handle value CurrentAddress if e.dataItem.Id !== STRING_EMPTY
                            if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                                $scope.model.CurrentAddress = $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                            } else {
                                $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                            }
                        }
                    });
                } else {
                    //handle value CurrentAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.CurrentAddress = $("#CurrentNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onSelectResidenceDistrict
            $scope.onSelectResidenceDistrict = function (e) {

                $scope.ResidenceWardsDropdownlist = [];

                // Set value string empty ResidenceWardsId
                $("#ResidenceWardsId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#ResidenceWardsId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#ResidenceWardsId").data("kendoDropDownList").trigger("change");
                $scope.model.ResidenceWardsId = STRING_EMPTY;

                //handle value ResidenceAddress
                $scope.model.ResidenceAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    ListCategoryService.GetAllWards(e.dataItem.Id).then(function (response) {
                        if (response.status === 200) {
                            $scope.ResidenceWardsDropdownlist = response.data;

                            //handle value ResidenceAddress if e.dataItem.Id !== STRING_EMPTY
                            if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                                $scope.model.ResidenceAddress = $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                            } else {
                                $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                            }
                        }
                    });
                } else {
                    //handle value ResidenceAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.ResidenceAddress = $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onSelectCurrentDistrict
            $scope.onSelectCurrentDistrict = function (e) {

                $scope.CurrentWardsDropdownlist = [];

                // Set value string empty CurrentWardsId
                $("#CurrentWardsId").data('kendoDropDownList').text(STRING_EMPTY);
                $("#CurrentWardsId").data('kendoDropDownList').value(STRING_EMPTY);
                $("#CurrentWardsId").data("kendoDropDownList").trigger("change");
                $scope.model.CurrentWardsId = STRING_EMPTY;

                //handle value CurrentAddress
                $scope.model.CurrentAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    ListCategoryService.GetAllWards(e.dataItem.Id).then(function (response) {
                        if (response.status === 200) {
                            $scope.CurrentWardsDropdownlist = response.data;

                            //handle value CurrentAddress if e.dataItem.Id !== STRING_EMPTY
                            if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                                $scope.model.CurrentAddress = $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                            } else {
                                $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                            }
                        }
                    });
                } else {
                    //handle value CurrentAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.CurrentAddress = $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onSelectResidenceWards
            $scope.onSelectResidenceWards = function (e) {

                //handle value ResidenceAddress
                $scope.model.ResidenceAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    //handle value ResidenceAddress if e.dataItem.Id !== STRING_EMPTY
                    if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.ResidenceAddress = e.dataItem.Name + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + e.dataItem.Name + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    }
                } else {
                    //handle value ResidenceAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#ResidenceHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.ResidenceAddress = $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.ResidenceAddress = $("#ResidenceHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onSelectCurrentWards
            $scope.onSelectCurrentWards = function (e) {

                //handle value CurrentAddress
                $scope.model.CurrentAddress = STRING_EMPTY;

                if (e.dataItem.Id !== STRING_EMPTY) {
                    //handle value CurrentAddress if e.dataItem.Id !== STRING_EMPTY
                    if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.CurrentAddress = e.dataItem.Name + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + e.dataItem.Name + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    }
                } else {
                    //handle value CurrentAddress if e.dataItem.Id === STRING_EMPTY
                    if ($("#CurrentHouseStreetVillageNumber").val() === STRING_EMPTY) {
                        $scope.model.CurrentAddress = $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    } else {
                        $scope.model.CurrentAddress = $("#CurrentHouseStreetVillageNumber").val() + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onChangeResidenceHouseStreetVillageNumber
            $scope.onChangeResidenceHouseStreetVillageNumber = function () {
                if ($scope.model.ResidenceHouseStreetVillageNumber != STRING_EMPTY) {
                    if ($scope.model.ResidenceWardsId === STRING_EMPTY || $scope.model.ResidenceWardsId === null) {

                        if (($scope.model.ResidenceDistrictId === STRING_EMPTY || $scope.model.ResidenceDistrictId === null) && ($scope.model.ResidenceProvinceCityId === STRING_EMPTY || $scope.model.ResidenceProvinceCityId === null) && ($scope.model.ResidenceNationalityId === STRING_EMPTY || $scope.model.ResidenceNationalityId === null)) {
                            $scope.model.ResidenceAddress = $scope.model.ResidenceHouseStreetVillageNumber;
                        } else if (($scope.model.ResidenceDistrictId === STRING_EMPTY || $scope.model.ResidenceDistrictId === null) && ($scope.model.ResidenceProvinceCityId === STRING_EMPTY || $scope.model.ResidenceProvinceCityId === null) && ($scope.model.ResidenceNationalityId !== STRING_EMPTY || $scope.model.ResidenceNationalityId !== null)) {
                            $scope.model.ResidenceAddress = $scope.model.ResidenceHouseStreetVillageNumber + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.ResidenceDistrictId === STRING_EMPTY || $scope.model.ResidenceDistrictId === null) && ($scope.model.ResidenceProvinceCityId !== STRING_EMPTY || $scope.model.ResidenceProvinceCityId !== null) && ($scope.model.ResidenceNationalityId !== STRING_EMPTY || $scope.model.ResidenceNationalityId !== null)) {
                            $scope.model.ResidenceAddress = $scope.model.ResidenceHouseStreetVillageNumber + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.ResidenceDistrictId !== STRING_EMPTY || $scope.model.ResidenceDistrictId !== null) && ($scope.model.ResidenceProvinceCityId !== STRING_EMPTY || $scope.model.ResidenceProvinceCityId !== null) && ($scope.model.ResidenceNationalityId !== STRING_EMPTY || $scope.model.ResidenceNationalityId !== null)) {
                            $scope.model.ResidenceAddress = $scope.model.ResidenceHouseStreetVillageNumber + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                        }

                    } else {
                        $scope.model.ResidenceAddress = $scope.model.ResidenceHouseStreetVillageNumber + COMMA_EMPTY + $("#ResidenceWardsId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    }
                } else {
                    if ($scope.model.ResidenceWardsId === STRING_EMPTY || $scope.model.ResidenceWardsId === null) {

                        if (($scope.model.ResidenceDistrictId === STRING_EMPTY || $scope.model.ResidenceDistrictId === null) && ($scope.model.ResidenceProvinceCityId === STRING_EMPTY || $scope.model.ResidenceProvinceCityId === null) && ($scope.model.ResidenceNationalityId === STRING_EMPTY || $scope.model.ResidenceNationalityId === null)) {
                            $scope.model.ResidenceAddress = $scope.model.ResidenceHouseStreetVillageNumber;
                        } else if (($scope.model.ResidenceDistrictId === STRING_EMPTY || $scope.model.ResidenceDistrictId === null) && ($scope.model.ResidenceProvinceCityId === STRING_EMPTY || $scope.model.ResidenceProvinceCityId === null) && ($scope.model.ResidenceNationalityId !== STRING_EMPTY || $scope.model.ResidenceNationalityId !== null)) {
                            $scope.model.ResidenceAddress = $("#ResidenceNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.ResidenceDistrictId === STRING_EMPTY || $scope.model.ResidenceDistrictId === null) && ($scope.model.ResidenceProvinceCityId !== STRING_EMPTY || $scope.model.ResidenceProvinceCityId !== null) && ($scope.model.ResidenceNationalityId !== STRING_EMPTY || $scope.model.ResidenceNationalityId !== null)) {
                            $scope.model.ResidenceAddress = $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.ResidenceDistrictId !== STRING_EMPTY || $scope.model.ResidenceDistrictId !== null) && ($scope.model.ResidenceProvinceCityId !== STRING_EMPTY || $scope.model.ResidenceProvinceCityId !== null) && ($scope.model.ResidenceNationalityId !== STRING_EMPTY || $scope.model.ResidenceNationalityId !== null)) {
                            $scope.model.ResidenceAddress = $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                        }

                    } else {
                        $scope.model.ResidenceAddress = $("#ResidenceWardsId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#ResidenceNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onChangeCurrentHouseStreetVillageNumber
            $scope.onChangeCurrentHouseStreetVillageNumber = function () {
                if ($scope.model.CurrentHouseStreetVillageNumber != STRING_EMPTY) {
                    if ($scope.model.CurrentWardsId === STRING_EMPTY || $scope.model.CurrentWardsId === null) {

                        if (($scope.model.CurrentDistrictId === STRING_EMPTY || $scope.model.CurrentDistrictId === null) && ($scope.model.CurrentProvinceCityId === STRING_EMPTY || $scope.model.CurrentProvinceCityId === null) && ($scope.model.CurrentNationalityId === STRING_EMPTY || $scope.model.CurrentNationalityId === null)) {
                            $scope.model.CurrentAddress = $scope.model.CurrentHouseStreetVillageNumber;
                        } else if (($scope.model.CurrentDistrictId === STRING_EMPTY || $scope.model.CurrentDistrictId === null) && ($scope.model.CurrentProvinceCityId === STRING_EMPTY || $scope.model.CurrentProvinceCityId === null) && ($scope.model.CurrentNationalityId !== STRING_EMPTY || $scope.model.CurrentNationalityId !== null)) {
                            $scope.model.CurrentAddress = $scope.model.CurrentHouseStreetVillageNumber + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.CurrentDistrictId === STRING_EMPTY || $scope.model.CurrentDistrictId === null) && ($scope.model.CurrentProvinceCityId !== STRING_EMPTY || $scope.model.CurrentProvinceCityId !== null) && ($scope.model.CurrentNationalityId !== STRING_EMPTY || $scope.model.CurrentNationalityId !== null)) {
                            $scope.model.CurrentAddress = $scope.model.CurrentHouseStreetVillageNumber + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.CurrentDistrictId !== STRING_EMPTY || $scope.model.CurrentDistrictId !== null) && ($scope.model.CurrentProvinceCityId !== STRING_EMPTY || $scope.model.CurrentProvinceCityId !== null) && ($scope.model.CurrentNationalityId !== STRING_EMPTY || $scope.model.CurrentNationalityId !== null)) {
                            $scope.model.CurrentAddress = $scope.model.CurrentHouseStreetVillageNumber + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                        }

                    } else {
                        $scope.model.CurrentAddress = $scope.model.CurrentHouseStreetVillageNumber + COMMA_EMPTY + $("#CurrentWardsId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    }
                } else {
                    if ($scope.model.CurrentWardsId === STRING_EMPTY || $scope.model.CurrentWardsId === null) {

                        if (($scope.model.CurrentDistrictId === STRING_EMPTY || $scope.model.CurrentDistrictId === null) && ($scope.model.CurrentProvinceCityId === STRING_EMPTY || $scope.model.CurrentProvinceCityId === null) && ($scope.model.CurrentNationalityId === STRING_EMPTY || $scope.model.CurrentNationalityId === null)) {
                            $scope.model.CurrentAddress = $scope.model.CurrentHouseStreetVillageNumber;
                        } else if (($scope.model.CurrentDistrictId === STRING_EMPTY || $scope.model.CurrentDistrictId === null) && ($scope.model.CurrentProvinceCityId === STRING_EMPTY || $scope.model.CurrentProvinceCityId === null) && ($scope.model.CurrentNationalityId !== STRING_EMPTY || $scope.model.CurrentNationalityId !== null)) {
                            $scope.model.CurrentAddress = $("#CurrentNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.CurrentDistrictId === STRING_EMPTY || $scope.model.CurrentDistrictId === null) && ($scope.model.CurrentProvinceCityId !== STRING_EMPTY || $scope.model.CurrentProvinceCityId !== null) && ($scope.model.CurrentNationalityId !== STRING_EMPTY || $scope.model.CurrentNationalityId !== null)) {
                            $scope.model.CurrentAddress = $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                        } else if (($scope.model.CurrentDistrictId !== STRING_EMPTY || $scope.model.CurrentDistrictId !== null) && ($scope.model.CurrentProvinceCityId !== STRING_EMPTY || $scope.model.CurrentProvinceCityId !== null) && ($scope.model.CurrentNationalityId !== STRING_EMPTY || $scope.model.CurrentNationalityId !== null)) {
                            $scope.model.CurrentAddress = $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                        }

                    } else {
                        $scope.model.CurrentAddress = $("#CurrentWardsId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentDistrictId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentProvinceCityId").data('kendoDropDownList').text() + COMMA_EMPTY + $("#CurrentNationalityId").data('kendoDropDownList').text();
                    }
                }
            };

            // onSelectPlaceOfRegistrationForMedicalExaminationAndTreatment
            $scope.onSelectPlaceOfRegistrationForMedicalExaminationAndTreatment = function (e) {
                $scope.model.CodesOfMedicalExaminationAndTreatmentPlaces = e.dataItem.Code;
            };

            // onSelectProvinceCity
            $scope.onSelectProvinceCity = function (e) {
                $scope.model.ProvinceCodeLevel = e.dataItem.Code;
            };

            // Load datasource for DirectManagementId autocomplete box
            $scope.GetEmployeeForDirectManagement = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.model.DirectManagementAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item DirectManagementId autocomplete box
            $scope.onSelectDirectManagementId = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForDirectManagement.data()[index];
                $scope.model.DirectManagementId = itemSelected.Id;
                $scope.model.DirectManagementAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#DirectManagementId").data("kendoAutoComplete").close();
                e.preventDefault();
            };

            // Event blur item DirectManagementId autocomplete box
            $scope.onChangeDirectManagementId = function () {
                if ($scope.model.DirectManagementAutoComplete === $scope.model.DirectManagementNameTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForDirectManagement.data().forEach(function (item) {
                    if ($scope.model.DirectManagementId === item.Id) {
                        $scope.model.DirectManagementAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.model.DirectManagementId = STRING_EMPTY;
                    $scope.model.DirectManagementAutoComplete = STRING_EMPTY;
                }
            };

            // onChangeDirectManagementIdValue
            $scope.onChangeDirectManagementIdValue = function () {
                if ($scope.model.DirectManagementAutoComplete === STRING_EMPTY) {
                    $scope.model.DirectManagementId = STRING_EMPTY;
                    $scope.model.DirectManagementAutoComplete = STRING_EMPTY;
                }
            };

            // Load datasource for IndirectManagementId autocomplete box
            $scope.GetEmployeeForIndirectManagement = new kendo.data.DataSource({
                type: "jsonp",
                serverFiltering: true,
                transport: {
                    read: function (options) {
                        EmployeeInformationService.GetEmployeeForAutoCompleBox($scope.model.IndirectManagementAutoComplete)
                            .success(function (data) {
                                options.success(data);
                            }).error(function () {
                                options.error();
                            });
                    }
                },
                schema: $.extend({}, kendo.data.schemas.webapi, { data: "Data", total: "Total", errors: "Errors" })
            });

            // Event onselect item IndirectManagementId autocomplete box
            $scope.onSelectIndirectManagementId = function (e) {
                var index = e.item.index();
                var itemSelected = $scope.GetEmployeeForIndirectManagement.data()[index];
                $scope.model.IndirectManagementId = itemSelected.Id;
                $scope.model.IndirectManagementAutoComplete = itemSelected.EmployeeName;
                $scope.$applyAsync();
                $("#IndirectManagementId").data("kendoAutoComplete").close();
                e.preventDefault();
            }

            // Event blur item IndirectManagementId autocomplete box
            $scope.onChangeIndirectManagementId = function () {
                if ($scope.model.IndirectManagementAutoComplete === $scope.model.IndirectManagementNameTemp) {
                    return;
                }

                var matchCode = false;
                $scope.GetEmployeeForIndirectManagement.data().forEach(function (item) {
                    if ($scope.model.IndirectManagementId === item.Id) {
                        $scope.model.IndirectManagementAutoComplete = item.EmployeeName;
                        matchCode = true;
                    }
                });
                if (!matchCode) {
                    $scope.model.IndirectManagementId = STRING_EMPTY;
                    $scope.model.IndirectManagementAutoComplete = STRING_EMPTY;
                }
            }

            // onChangeIndirectManagementIdValue
            $scope.onChangeIndirectManagementIdValue = function () {
                if ($scope.model.IndirectManagementAutoComplete === STRING_EMPTY) {
                    $scope.model.IndirectManagementId = STRING_EMPTY;
                    $scope.model.IndirectManagementAutoComplete = STRING_EMPTY;
                }
            };

            // Save
            $scope.Save = function (form) {
                $scope.IsSave = true;

                // Check validate form

                if (($scope.model.WorkUnitId === STRING_EMPTY || !$scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName === STRING_EMPTY || !$scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId === STRING_EMPTY || !$scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Đơn vị công tác \n - Họ và Tên \n - Vị trí công việc");
                    $window.scrollTo(0, 0);
                    return;
                } else if (($scope.model.WorkUnitId === STRING_EMPTY || !$scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName === STRING_EMPTY || !$scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId !== STRING_EMPTY || $scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Đơn vị công tác \n - Họ và Tên");
                    $window.scrollTo(0, 0);
                    return;
                } else if (($scope.model.WorkUnitId === STRING_EMPTY || !$scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName !== STRING_EMPTY || $scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId === STRING_EMPTY || !$scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Đơn vị công tác \n - Vị trí công việc");
                    $window.scrollTo(0, 0);
                    return;
                } else if (($scope.model.WorkUnitId !== STRING_EMPTY || $scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName === STRING_EMPTY || !$scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId === STRING_EMPTY || !$scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Họ và Tên \n - Vị trí công việc");
                    $window.scrollTo(0, 0);
                    return;
                } else if (($scope.model.WorkUnitId === STRING_EMPTY || !$scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName !== STRING_EMPTY || $scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId !== STRING_EMPTY || $scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Đơn vị công tác");
                    $window.scrollTo(0, 0);
                    return;
                } else if (($scope.model.WorkUnitId !== STRING_EMPTY || $scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName === STRING_EMPTY || !$scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId !== STRING_EMPTY || $scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Họ và Tên");
                    $window.scrollTo(0, 0);
                    return;
                } else if (($scope.model.WorkUnitId !== STRING_EMPTY || $scope.model.WorkUnitId) &&
                    ($scope.model.EmployeeName !== STRING_EMPTY || $scope.model.EmployeeName) &&
                    ($scope.model.JobPositionId === STRING_EMPTY || !$scope.model.JobPositionId)) {
                    window.alert("Thông tin bắt buộc : \n - Vị trí công việc");
                    $window.scrollTo(0, 0);
                    return;
                }

                var data = angular.copy($scope.model);
                data.DateOfBirth = kendo.parseDate($scope.model.DateOfBirth, DATE_FORMAT);
                data.DateOfIssueOfIdentificationCard = kendo.parseDate($scope.model.DateOfIssueOfIdentificationCard, DATE_FORMAT);
                data.IdentificationCardExpirationDate = kendo.parseDate($scope.model.IdentificationCardExpirationDate, DATE_FORMAT);
                data.PassportDate = kendo.parseDate($scope.model.PassportDate, DATE_FORMAT);
                data.PassportExpirationDate = kendo.parseDate($scope.model.PassportExpirationDate, DATE_FORMAT);
                data.ApprenticeDay = kendo.parseDate($scope.model.ApprenticeDay, DATE_FORMAT);
                data.ProbationDay = kendo.parseDate($scope.model.ProbationDay, DATE_FORMAT);
                data.OfficialDate = kendo.parseDate($scope.model.OfficialDate, DATE_FORMAT);
                data.DateOfInsurance = kendo.parseDate($scope.model.DateOfInsurance, DATE_FORMAT);
                data.HealthInsuranceExpirationDate = kendo.parseDate($scope.model.HealthInsuranceExpirationDate, DATE_FORMAT);

                // Check validate DateOfIssueOfIdentificationCard < IdentificationCardExpirationDate
                if (data.DateOfIssueOfIdentificationCard != null && data.IdentificationCardExpirationDate != null && data.DateOfIssueOfIdentificationCard.getTime() > data.IdentificationCardExpirationDate.getTime()) {
                    $scope.CheckDateOfIssueOfIdentificationCardLessIdentificationCardExpirationDate = true;
                    $window.scrollTo(0, 0);
                    return;
                } else {
                    $scope.CheckDateOfIssueOfIdentificationCardLessIdentificationCardExpirationDate = false;
                }


                // Check validate PassportDate < PassportExpirationDate
                if (data.PassportDate != null && data.PassportExpirationDate != null && data.PassportDate.getTime() > data.PassportExpirationDate.getTime()) {
                    $scope.CheckPassportDateLessPassportExpirationDate = true;
                    $window.scrollTo(0, 0);
                    return;
                }
                else {
                    $scope.CheckPassportDateLessPassportExpirationDate = false;
                }

                // Check validate ApprenticeDay < ProbationDay
                if (data.ApprenticeDay != null && data.ProbationDay != null && data.ApprenticeDay.getTime() > data.ProbationDay.getTime()) {
                    $scope.CheckApprenticeDayLessProbationDay = true;
                    $window.scrollTo(0, 0);
                    return;
                } else {
                    $scope.CheckApprenticeDayLessProbationDay = false;
                }

                // Check validate ProbationDay < OfficialDate
                if (data.ProbationDay != null && data.OfficialDate != null && data.ProbationDay.getTime() > data.OfficialDate.getTime()) {
                    $scope.CheckProbationDayLessOfficialDate = true;
                    $window.scrollTo(0, 0);
                    return;
                } else {
                    $scope.CheckProbationDayLessOfficialDate = false;
                }

                var dataPoliticsHealthMilitary = angular.copy($scope.modelPoliticsHealthMilitary);
                dataPoliticsHealthMilitary.DayToUnion = kendo.parseDate($scope.modelPoliticsHealthMilitary.DayToUnion, DATE_FORMAT);
                dataPoliticsHealthMilitary.DateOfEnlistment = kendo.parseDate($scope.modelPoliticsHealthMilitary.DateOfEnlistment, DATE_FORMAT);
                dataPoliticsHealthMilitary.DateOfDemobilization = kendo.parseDate($scope.modelPoliticsHealthMilitary.DateOfDemobilization, DATE_FORMAT);
                dataPoliticsHealthMilitary.DayToParty = kendo.parseDate($scope.modelPoliticsHealthMilitary.DayToParty, DATE_FORMAT);
                dataPoliticsHealthMilitary.DateToJoinRevolution = kendo.parseDate($scope.modelPoliticsHealthMilitary.DateToJoinRevolution, DATE_FORMAT);

                var dataSkin = angular.copy($scope.modelSkin);

                loading();
                EmployeeInformationService.SaveProfile(data).then(function success(response) {
                    if (response.data.status === 200) {
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoading();
                        if (data.Id > 0) {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                        }
                        else {
                            $scope.ShowList = true;
                            $scope.ShowInfo = false;
                            $scope.IdTemp = 0;
                        }
                        $scope.CheckDateOfIssueOfIdentificationCardLessIdentificationCardExpirationDate = false;
                        $scope.CheckPassportDateLessPassportExpirationDate = false;
                        $scope.CheckApprenticeDayLessProbationDay = false;
                        $scope.CheckProbationDayLessOfficialDate = false;

                        // Save PoliticsHealthMilitary
                        PoliticsHealthMilitaryInformationService.SavePoliticsHealthMilitaryInformation(dataPoliticsHealthMilitary).then(function success(response) {
                            if (response.data.status === 200) {
                                stopLoading();
                            } else {
                                stopLoading();
                            }
                        });

                        // Save SkinInformation
                        SkinInformationService.SaveSkinInformation(dataSkin).then(function success(response) {
                            if (response.data.status === 200) {
                                stopLoading();
                            } else {
                                stopLoading();
                            }
                        });
                    } else {
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoading();
                        if (data.Id > 0) {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                        }
                        else {
                            $scope.ShowList = false;
                            $scope.ShowInfo = true;
                            $scope.IdTemp = 0;
                        }
                    }
                });
            };

            // ClickInformation
            $scope.ClickInformation = function (e) {
                switch (e) {
                    case "EMPLOYEE":
                        loadingProfilePage();
                        // check show hide button save by workstatus
                        $scope.ShowHideButton();
                        EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.Success === true) {
                                $scope.model.Id = $scope.IdTemp;
                                $scope.model.Image = response.data.Data.Image;
                                $scope.model.EmployeeCode = response.data.Data.EmployeeCode;
                                $scope.model.EmployeeName = response.data.Data.EmployeeName;
                                $scope.model.SexId = response.data.Data.SexId;
                                $scope.model.DateOfBirth = kendo.parseDate(response.data.Data.DateOfBirth, DATE_FORMAT);
                                $scope.model.PersonalTaxCode = response.data.Data.PersonalTaxCode;
                                $scope.model.WorkUnitId = response.data.Data.WorkUnitId;
                                $scope.model.JobPositionId = response.data.Data.JobPositionId;
                                $scope.model.NationId = response.data.Data.NationId;
                                $scope.model.ReligionId = response.data.Data.ReligionId;
                                $scope.model.NationalityId = response.data.Data.NationalityId;
                                $scope.model.IdentificationCardNumber = response.data.Data.IdentificationCardNumber;
                                $scope.model.DateOfIssueOfIdentificationCard = kendo.parseDate(response.data.Data.DateOfIssueOfIdentificationCard, DATE_FORMAT);
                                $scope.model.PlaceOfIssueOfIdCard = response.data.Data.PlaceOfIssueOfIdCard;
                                $scope.model.IdentificationCardExpirationDate = kendo.parseDate(response.data.Data.IdentificationCardExpirationDate, DATE_FORMAT);
                                $scope.model.PassportNumber = response.data.Data.PassportNumber;
                                $scope.model.PassportDate = kendo.parseDate(response.data.Data.PassportDate, DATE_FORMAT);
                                $scope.model.PlaceOfIssueOfPassport = response.data.Data.PlaceOfIssueOfPassport;
                                $scope.model.PassportExpirationDate = kendo.parseDate(response.data.Data.PassportExpirationDate, DATE_FORMAT);
                                $scope.model.EducationalLevelId = response.data.Data.EducationalLevelId;
                                $scope.model.DegreeTrainingId = response.data.Data.DegreeTrainingId;
                                $scope.model.TrainingPlacesId = response.data.Data.TrainingPlacesId;
                                $scope.model.FacultyId = response.data.Data.FacultyId;
                                $scope.model.SpecializedId = response.data.Data.SpecializedId;
                                $scope.model.GraduationYear = response.data.Data.GraduationYear;
                                $scope.model.ClassificationId = response.data.Data.ClassificationId;
                                $scope.model.MaritalStatusId = response.data.Data.MaritalStatusId;
                                $scope.model.FamilyMemberId = response.data.Data.FamilyMemberId;
                                $scope.model.IngredientsThemselvesId = response.data.Data.IngredientsThemselvesId;
                                stopLoadingProfilePage();
                            } else {
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "CONTACT":
                        loadingProfilePage();
                        // check show hide button save by workstatus
                        $scope.ShowHideButton();
                        EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.Success === true) {
                                $scope.model.Id = $scope.IdTemp;
                                $scope.model.MobilePhone = response.data.Data.MobilePhone;
                                $scope.model.OfficePhone = response.data.Data.OfficePhone;
                                $scope.model.HomePhone = response.data.Data.HomePhone;
                                $scope.model.OtherPhone = response.data.Data.OtherPhone;
                                $scope.model.PersonalEmail = response.data.Data.PersonalEmail;
                                $scope.model.CompanyEmail = response.data.Data.CompanyEmail;
                                $scope.model.OtherEmail = response.data.Data.OtherEmail;
                                $scope.model.Skype = response.data.Data.Skype;
                                $scope.model.Facebook = response.data.Data.Facebook;
                                $scope.model.Domicile = response.data.Data.Domicile;
                                $scope.model.ProvinceCityId = response.data.Data.ProvinceCityId;
                                $scope.model.PlaceBirth = response.data.Data.PlaceBirth;
                                $scope.model.ResidenceNationalityId = response.data.Data.ResidenceNationalityId;
                                $scope.model.ResidenceProvinceCityId = response.data.Data.ResidenceProvinceCityId;
                                $scope.model.ResidenceDistrictId = response.data.Data.ResidenceDistrictId;
                                $scope.model.ResidenceWardsId = response.data.Data.ResidenceWardsId;
                                $scope.model.ResidenceHouseStreetVillageNumber = response.data.Data.ResidenceHouseStreetVillageNumber;
                                $scope.model.ResidenceAddress = response.data.Data.ResidenceAddress;
                                $scope.model.ResidenceHouseholdRegistrationNumber = response.data.Data.ResidenceHouseholdRegistrationNumber;
                                $scope.model.ResidenceHouseholdCode = response.data.Data.ResidenceHouseholdCode;
                                $scope.model.ResidenceIsHeadHousehold = response.data.Data.ResidenceIsHeadHousehold;
                                $scope.model.CurrentNationalityId = response.data.Data.CurrentNationalityId;
                                $scope.model.CurrentProvinceCityId = response.data.Data.CurrentProvinceCityId;
                                $scope.model.CurrentDistrictId = response.data.Data.CurrentDistrictId;
                                $scope.model.CurrentWardsId = response.data.Data.CurrentWardsId;
                                $scope.model.CurrentHouseStreetVillageNumber = response.data.Data.CurrentHouseStreetVillageNumber;
                                $scope.model.CurrentAddress = response.data.Data.CurrentAddress;
                                $scope.model.UrgentContactFirstAndLastName = response.data.Data.UrgentContactFirstAndLastName;
                                $scope.model.UrgentContactRelationshipId = response.data.Data.UrgentContactRelationshipId;
                                $scope.model.UrgentContactMobilePhone = response.data.Data.UrgentContactMobilePhone;
                                $scope.model.UrgentContactHomePhone = response.data.Data.UrgentContactHomePhone;
                                $scope.model.UrgentContactEmail = response.data.Data.UrgentContactEmail;
                                $scope.model.UrgentContactAddress = response.data.Data.UrgentContactAddress;
                                stopLoadingProfilePage();
                            } else {
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "JOB":
                        loadingProfilePage();
                        // check show hide button save by workstatus
                        $scope.ShowHideButton();
                        EmployeeInformationService.GetEmployeeInformationById($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.Success === true) {
                                $scope.model.Id = $scope.IdTemp;
                                $scope.model.TimekeepingCode = response.data.Data.TimekeepingCode;
                                $scope.model.WorkStatusId = response.data.Data.WorkStatusId;
                                $scope.model.DirectManagementId = response.data.Data.DirectManagementId;
                                $scope.model.IndirectManagementId = response.data.Data.IndirectManagementId;
                                $scope.model.WorkLocationId = response.data.Data.WorkLocationId;
                                $scope.model.LaborManagementBookNumber = response.data.Data.LaborManagementBookNumber;
                                $scope.model.ContractTypeId = response.data.Data.ContractTypeId;
                                $scope.model.ApprenticeDay = kendo.parseDate(response.data.Data.ApprenticeDay, DATE_FORMAT);
                                $scope.model.ProbationDay = kendo.parseDate(response.data.Data.ProbationDay, DATE_FORMAT);
                                $scope.model.OfficialDate = kendo.parseDate(response.data.Data.OfficialDate, DATE_FORMAT);
                                $scope.model.NumberOfDaysOff = response.data.Data.NumberOfDaysOff;
                                $scope.model.AutomaticallyIncreasesMagicAccordingToSeniority = response.data.Data.AutomaticallyIncreasesMagicAccordingToSeniority;
                                $scope.model.IncreaseLaterSpells = response.data.Data.IncreaseLaterSpells;
                                $scope.model.WageId = response.data.Data.WageId;
                                $scope.model.BasicSalary = response.data.Data.BasicSalary;
                                $scope.model.InsurancePremiums = response.data.Data.InsurancePremiums;
                                $scope.model.StandardPublicNumber = response.data.Data.StandardPublicNumber;
                                $scope.model.StandardPublicId = response.data.Data.StandardPublicId;
                                $scope.model.BankAccoun = response.data.Data.BankAccoun;
                                $scope.model.BankId = response.data.Data.BankId;
                                $scope.model.JoinTheUnion = response.data.Data.JoinTheUnion;
                                $scope.model.DateOfInsurance = kendo.parseDate(response.data.Data.DateOfInsurance, DATE_FORMAT);
                                $scope.model.InsurancePremiumRate = response.data.Data.InsurancePremiumRate;
                                $scope.model.SomeSocialInsuranceBooks = response.data.Data.SomeSocialInsuranceBooks;
                                $scope.model.SocialInsuranceNumber = response.data.Data.SocialInsuranceNumber;
                                $scope.model.ProvinceCodeLevel = response.data.Data.ProvinceCodeLevel;
                                $scope.model.ProvinceNameLevelId = response.data.Data.ProvinceNameLevelId;
                                $scope.model.HealthInsuranceCardNumber = response.data.Data.HealthInsuranceCardNumber;
                                $scope.model.HealthInsuranceExpirationDate = kendo.parseDate(response.data.Data.HealthInsuranceExpirationDate, DATE_FORMAT);
                                $scope.model.PlaceOfRegistrationForMedicalExaminationAndTreatmentId = response.data.Data.PlaceOfRegistrationForMedicalExaminationAndTreatmentId;
                                $scope.model.CodesOfMedicalExaminationAndTreatmentPlaces = response.data.Data.CodesOfMedicalExaminationAndTreatmentPlaces;
                                $scope.model.DirectManagementAutoComplete = response.data.Data.DirectManagementName;
                                $scope.model.IndirectManagementAutoComplete = response.data.Data.IndirectManagementName;
                                $scope.model.DirectManagementNameTemp = response.data.Data.DirectManagementName;
                                $scope.model.IndirectManagementNameTemp = response.data.Data.IndirectManagementName;
                                stopLoadingProfilePage();
                            } else {
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "FAMILY":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        FamilyInformationService.GetAllFamilyInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllFamilyInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllFamilyInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "POLITICSHEALTHMILITARY":
                        loadingProfilePage();
                        // check show hide button save by workstatus
                        $scope.ShowHideButton();
                        BindDataToDropdownlistForPoliticsHealthMilitaryInformation();
                        $scope.modelPoliticsHealthMilitary = {
                            Id: 0,
                            EmployeeId: STRING_EMPTY,
                            IsAUnionMember: false,
                            DayToUnion: STRING_EMPTY,
                            GroupPositionId: STRING_EMPTY,
                            PlaceOfUnionAdmission: STRING_EMPTY,
                            AsAPartyMember: false,
                            DayToParty: STRING_EMPTY,
                            PartyPositionId: STRING_EMPTY,
                            PlaceOfAdmissionToTheParty: STRING_EMPTY,
                            BloodGroupId: STRING_EMPTY,
                            Height: STRING_EMPTY,
                            Weight: STRING_EMPTY,
                            HealthStatus: STRING_EMPTY,
                            Diseases: STRING_EMPTY,
                            Note: STRING_EMPTY,
                            PeopleWithDisabilities: false,
                            AsASsoldier: false,
                            DateOfEnlistment: STRING_EMPTY,
                            ArmyId: STRING_EMPTY,
                            MilitaryUnit: STRING_EMPTY,
                            MilitaryRankId: STRING_EMPTY,
                            MilitaryPositionId: STRING_EMPTY,
                            DateOfDemobilization: STRING_EMPTY,
                            TheReason: STRING_EMPTY,
                            AsWoundedSoldiersSickSoldiers: false,
                            DateToJoinRevolution: STRING_EMPTY,
                            RankId: STRING_EMPTY,
                            RateOfLaborDecline: STRING_EMPTY,
                            EnjoyTheMode: false
                        };
                        PoliticsHealthMilitaryInformationService.GetAllPoliticsHealthMilitaryInformation($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelPoliticsHealthMilitary.EmployeeId = $scope.IdTemp;
                                $scope.modelPoliticsHealthMilitary.IsAUnionMember = response.data[0].IsAUnionMember;
                                $scope.modelPoliticsHealthMilitary.DayToUnion = kendo.parseDate(response.data[0].DayToUnion, DATE_FORMAT);
                                $scope.modelPoliticsHealthMilitary.GroupPositionId = response.data[0].GroupPositionId;
                                $scope.modelPoliticsHealthMilitary.PlaceOfUnionAdmission = response.data[0].PlaceOfUnionAdmission;
                                $scope.modelPoliticsHealthMilitary.AsAPartyMember = response.data[0].AsAPartyMember;
                                $scope.modelPoliticsHealthMilitary.DayToParty = kendo.parseDate(response.data[0].DayToParty, DATE_FORMAT);
                                $scope.modelPoliticsHealthMilitary.PartyPositionId = response.data[0].PartyPositionId;
                                $scope.modelPoliticsHealthMilitary.PlaceOfAdmissionToTheParty = response.data[0].PlaceOfAdmissionToTheParty;
                                $scope.modelPoliticsHealthMilitary.BloodGroupId = response.data[0].BloodGroupId;
                                $scope.modelPoliticsHealthMilitary.Height = response.data[0].Height;
                                $scope.modelPoliticsHealthMilitary.Weight = response.data[0].Weight;
                                $scope.modelPoliticsHealthMilitary.HealthStatus = response.data[0].HealthStatus;
                                $scope.modelPoliticsHealthMilitary.Diseases = response.data[0].Diseases;
                                $scope.modelPoliticsHealthMilitary.Note = response.data[0].Note;
                                $scope.modelPoliticsHealthMilitary.PeopleWithDisabilities = response.data[0].PeopleWithDisabilities;
                                $scope.modelPoliticsHealthMilitary.AsASsoldier = response.data[0].AsASsoldier;
                                $scope.modelPoliticsHealthMilitary.DateOfEnlistment = kendo.parseDate(response.data[0].DateOfEnlistment, DATE_FORMAT);
                                $scope.modelPoliticsHealthMilitary.ArmyId = response.data[0].ArmyId;
                                $scope.modelPoliticsHealthMilitary.MilitaryUnit = response.data[0].MilitaryUnit;
                                $scope.modelPoliticsHealthMilitary.MilitaryRankId = response.data[0].MilitaryRankId;
                                $scope.modelPoliticsHealthMilitary.MilitaryPositionId = response.data[0].MilitaryPositionId;
                                $scope.modelPoliticsHealthMilitary.DateOfDemobilization = kendo.parseDate(response.data[0].DateOfDemobilization, DATE_FORMAT);
                                $scope.modelPoliticsHealthMilitary.TheReason = response.data[0].TheReason;
                                $scope.modelPoliticsHealthMilitary.AsWoundedSoldiersSickSoldiers = response.data[0].AsWoundedSoldiersSickSoldiers;
                                $scope.modelPoliticsHealthMilitary.DateToJoinRevolution = kendo.parseDate(response.data[0].DateToJoinRevolution, DATE_FORMAT);
                                $scope.modelPoliticsHealthMilitary.RankId = response.data[0].RankId;
                                $scope.modelPoliticsHealthMilitary.RateOfLaborDecline = response.data[0].RateOfLaborDecline;
                                $scope.modelPoliticsHealthMilitary.EnjoyTheMode = response.data[0].EnjoyTheMode;
                                stopLoadingProfilePage();
                            } else {
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "WORKPROGRESS":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        WorkProgressInformationService.GetAllWorkProgressInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllWorkProgressInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllWorkProgressInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "CONTRACT":
                        loadingProfilePage();
                        BindDataToDropdownlistForContractInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        ContractInformationService.GetAllContractInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllContractInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllContractInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "SALARYHISTORY":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        SalaryHistoryInformationService.GetAllSalaryHistoryInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllSalaryHistoryInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllSalaryHistoryInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "BONUS":
                        loadingProfilePage();
                        BindDataToDropdownlistForBonusInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        BonusInformationService.GetAllBonusInformation($scope.IdTemp).then(function (response) {
                            $scope.GetAllBonusInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllBonusInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "PROBLEM":
                        loadingProfilePage();
                        BindDataToDropdownlistForProblemInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        ProblemInformationService.GetAllProblemInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllProblemInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllProblemInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "TRAININGPROCESS":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        TrainingProcessInformationService.GetAllTrainingProcessInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllTrainingProcessInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllTrainingProcessInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "EVALUATE":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        EvaluateService.GetEvaluateByEmployee(0, $scope.IdTemp).then(function (response) {
                            $scope.GetEvaluateByEmployee = [];
                            if (response.status === 200) {
                                $scope.GetEvaluateByEmployee = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "DEGREE":
                        loadingProfilePage();
                        BindDataToDropdownlistForDegreeInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        DegreeInformationService.GetAllDegreeInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllDegreeInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllDegreeInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "CERTIFICATE":
                        loadingProfilePage();
                        BindDataToDropdownlistForCertificateInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        CertificateInformationService.GetAllCertificateInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllCertificateInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllCertificateInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "WORKEXPERIENCE":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        WorkExperienceInformationService.GetAllWorkExperienceInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllWorkExperienceInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllWorkExperienceInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "SKILL":
                        loadingProfilePage();
                        BindDataToDropdownlistForSkillInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        SkillInformationService.GetAllSkillInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllSkillInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllSkillInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "RECEIVE":
                        loadingProfilePage();
                        BindDataToDropdownlistForReceiveInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        ReceiveInformationService.GetAllReceiveInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllReceiveInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllReceiveInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "QUIT":
                        loadingProfilePage();
                        BindDataToDropdownlistForQuitInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        QuitInformationService.GetAllQuitInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllQuitInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllQuitInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "ASSET":
                        loadingProfilePage();
                        BindDataToDropdownlistForAssetInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        AssetInformationService.GetAllAssetInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllAssetInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllAssetInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "SKIN":
                        loadingProfilePage();
                        // check show hide button save by workstatus
                        $scope.ShowHideButton();
                        BindDataToDropdownlistForSkinInformation();
                        $scope.modelSkin = {
                            Id: 0,
                            EmployeeId: STRING_EMPTY,
                            ShirtStringId: STRING_EMPTY,
                            TrousersStringId: STRING_EMPTY,
                            ZuypStringId: STRING_EMPTY,
                            ProtectiveGearStringId: STRING_EMPTY,
                            ShirtNumberId: STRING_EMPTY,
                            TrousersNumberId: STRING_EMPTY,
                            ZuypNumberId: STRING_EMPTY,
                            ProtectiveGearNumberId: STRING_EMPTY,
                            ShoulderWidth: STRING_EMPTY,
                            LongSleeve: STRING_EMPTY,
                            LongCoat: STRING_EMPTY,
                            ChestRing: STRING_EMPTY,
                            Waist: STRING_EMPTY,
                            Buttocks: STRING_EMPTY,
                            LongPants: STRING_EMPTY,
                            LongSkirt: STRING_EMPTY,
                            LapThigh: STRING_EMPTY
                        };
                        SkinInformationService.GetAllSkinInformation($scope.IdTemp).then(function (response) {
                            if (response.status === 200 && response.data.length > 0) {
                                $scope.modelSkin.EmployeeId = $scope.IdTemp;
                                $scope.modelSkin.ShirtStringId = response.data[0].ShirtStringId;
                                $scope.modelSkin.TrousersStringId = response.data[0].TrousersStringId;
                                $scope.modelSkin.ZuypStringId = response.data[0].ZuypStringId;
                                $scope.modelSkin.ProtectiveGearStringId = response.data[0].ProtectiveGearStringId;
                                $scope.modelSkin.ShirtNumberId = response.data[0].ShirtNumberId;
                                $scope.modelSkin.TrousersNumberId = response.data[0].TrousersNumberId;
                                $scope.modelSkin.ZuypNumberId = response.data[0].ZuypNumberId;
                                $scope.modelSkin.ProtectiveGearNumberId = response.data[0].ProtectiveGearNumberId;
                                $scope.modelSkin.ShoulderWidth = response.data[0].ShoulderWidth;
                                $scope.modelSkin.LongSleeve = response.data[0].LongSleeve;
                                $scope.modelSkin.LongCoat = response.data[0].LongCoat;
                                $scope.modelSkin.ChestRing = response.data[0].ChestRing;
                                $scope.modelSkin.Waist = response.data[0].Waist;
                                $scope.modelSkin.Buttocks = response.data[0].Buttocks;
                                $scope.modelSkin.LongPants = response.data[0].LongPants;
                                $scope.modelSkin.LongSkirt = response.data[0].LongSkirt;
                                $scope.modelSkin.LapThigh = response.data[0].LapThigh;
                                stopLoadingProfilePage();
                            } else {
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "PAGE":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        PageInformationService.GetAllPageInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllPageInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllPageInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "ATTACHMENT":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        AttachmentInformationService.GetAllAttachmentInformation(0, $scope.IdTemp, 0).then(function (response) {
                            $scope.GetAllAttachmentInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllAttachmentInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "ALLOWANCE":
                        loadingProfilePage();
                        BindDataToDropdownlistForAllowanceInformation();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        AllowanceInformationService.GetAllAllowanceInformation(0, $scope.IdTemp).then(function (response) {
                            $scope.GetAllAllowanceInformation = [];
                            if (response.status === 200) {
                                $scope.GetAllAllowanceInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    case "EMPLOYEES_ON_BUSINESS_TRIP_INFORMATION":
                        loadingProfilePage();
                        $scope.ShowHideButton();
                        $('.save-hide').addClass('hide');
                        EmployeesOnBusinessTripService.GetEmployeesOnBusinessTripInformation($scope.IdTemp).then(function (response) {
                            $scope.GetEmployeesOnBusinessTripInformation = [];
                            if (response.status === 200) {
                                $scope.GetEmployeesOnBusinessTripInformation = response.data;
                                stopLoadingProfilePage();
                            }
                        });
                        break;
                    default:
                        break;
                }
            };

            // CloseResignationProcedures
            $scope.CloseResignationProcedures = function () {
                //$scope.IsSaveAllowance = false;
                //onShowMessageValidate();
                $("#KenWindownResignationProcedures").closest(".k-window-content").data("kendoWindow").close();
            };

            // SaveResignationProcedures
            $scope.SaveResignationProcedures = function () {

                var data = angular.copy($scope.modelResignationProcedures);
                data.EmployeeId = $scope.IdTemp;
                data.ExpectedResignationDate = kendo.parseDate($scope.modelResignationProcedures.ExpectedResignationDate, DATE_FORMAT);

                loadingPopUp();
                $("#btnSaveResignationProcedures").prop("disabled", true);
                $("#CloseResignationProcedures").prop("disabled", true);
                ResignationProceduresService.SaveResignationProcedures(data).then(function success(response) {
                    if (response.data.status === 200) {
                        $("#KenWindownResignationProcedures").data("kendoWindow").close();
                        bootbox.alert("<span style='color:green; text-align:justify;'>" + response.data.message + "</span>");
                        RefreshKendoGrid("Table");
                        stopLoadingPopUp();
                    } else {
                        $("#KenWindownResignationProcedures").data("kendoWindow").close();
                        bootbox.alert("<span style='color:red; text-align:justify;'>" + response.data.message + "</span>");
                        stopLoadingPopUp();
                    }
                });
            };

            $scope.ShowHideButton = function () {
                // 2 = Đang làm thủ tục thôi việc, 3 = Nghỉ hưu, 8 = Nghỉ việc
                if ($scope.WorkStatusTemp === 2 || $scope.WorkStatusTemp === 3 || $scope.WorkStatusTemp === 8) {
                    $scope.HideButton = false;
                    $scope.ShowButton = true;
                } else {
                    $scope.HideButton = true;
                    $scope.ShowButton = false;
                }
            };

            // Check valid
            $scope.formHasError = function () {
                var hasErrors = false;
                for (var input in $scope.formErrors) {
                    hasErrors = hasErrors || $scope.formErrors[input];
                }
                hasErrors = hasErrors;
                return hasErrors;
            };

            $scope.showErrorMsg = function (input) {
                if (!input) return false;
                if ($scope.IsSave) input.$touched = $scope.IsSave;

                var hasError = (input.$touched || $scope.IsSave) && (input.$error.required || input.$error.invalid);
                $scope.formErrors[input.$name] = hasError;
                return hasError;
            };

        }]);

})(window.angular);
