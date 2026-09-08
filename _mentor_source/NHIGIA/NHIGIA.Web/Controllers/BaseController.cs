using GemBox.Spreadsheet;
using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Helper;
using NHIGIA.Services.Implementations;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace NHIGIA.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public readonly ListCategoryTypeService _listCategoryTypeService = new ListCategoryTypeService();
        public readonly ListCategoryService _listCategoryService = new ListCategoryService();
        public readonly BulkUploadService _bulkUploadService = new BulkUploadService();
        public readonly EmployeeInformationService _employeeInformationService = new EmployeeInformationService();
        public readonly FamilyInformationService _familyInformationService = new FamilyInformationService();
        public readonly PoliticsHealthMilitaryInformationService _politicsHealthMilitaryInformationService = new PoliticsHealthMilitaryInformationService();
        public readonly WorkProgressInformationService _workProgressInformationService = new WorkProgressInformationService();
        public readonly ContractInformationService _contractInformationService = new ContractInformationService();
        public readonly SalaryHistoryInformationService _salaryHistoryInformationService = new SalaryHistoryInformationService();
        public readonly BonusInformationService _bonusInformationService = new BonusInformationService();
        public readonly ProblemInformationService _problemInformationService = new ProblemInformationService();
        public readonly TrainingProcessInformationService _trainingProcessInformationService = new TrainingProcessInformationService();
        public readonly DegreeInformationService _degreeInformationService = new DegreeInformationService();
        public readonly CertificateInformationService _certificateInformationService = new CertificateInformationService();
        public readonly WorkExperienceInformationService _workExperienceInformationService = new WorkExperienceInformationService();
        public readonly SkillInformationService _skillInformationService = new SkillInformationService();
        public readonly ReceiveInformationService _receiveInformationService = new ReceiveInformationService();
        public readonly QuitInformationService _quitInformationService = new QuitInformationService();
        public readonly AssetInformationService _assetInformationService = new AssetInformationService();
        public readonly SkinInformationService _skinInformationService = new SkinInformationService();
        public readonly PageInformationService _pageInformationService = new PageInformationService();
        public readonly AttachmentInformationService _attachmentInformationService = new AttachmentInformationService();
        public readonly EvaluateService _evaluateService = new EvaluateService();
        public readonly AllowanceInformationService _allowanceInformationService = new AllowanceInformationService();
        public readonly EmployeesOnBusinessTripService _employeesOnBusinessTripService = new EmployeesOnBusinessTripService();
        public readonly EmployeesOnBusinessTripAdvancesService _employeesOnBusinessTripAdvancesService = new EmployeesOnBusinessTripAdvancesService();
        public readonly EmployeesOnBusinessTripPaymentsService _employeesOnBusinessTripPaymentsService = new EmployeesOnBusinessTripPaymentsService();
        public readonly EmployeesOnBusinessTripRevenueEstimatesService _employeesOnBusinessTripRevenueEstimatesService = new EmployeesOnBusinessTripRevenueEstimatesService();
        public readonly EmployeesOnBusinessTripAssignedStaffService _employeesOnBusinessTripAssignedStaffService = new EmployeesOnBusinessTripAssignedStaffService();
        public readonly ResignationProceduresService _resignationProceduresService = new ResignationProceduresService();
        public readonly ResignationProceduresEmployeeDebtService _resignationProceduresEmployeeDebtService = new ResignationProceduresEmployeeDebtService();
        public readonly BonusInformationStaffWereCommendedService _bonusInformationStaffWereCommendedService = new BonusInformationStaffWereCommendedService();
        public readonly ProblemInformationRelatedStaffService _problemInformationRelatedStaffService = new ProblemInformationRelatedStaffService();
        public readonly ProblemInformationTrackEmployeeCompensationService _problemInformationTrackEmployeeCompensationService = new ProblemInformationTrackEmployeeCompensationService();

        protected List<TypeSortDescriptor> CreateSortingParameters(string sortColumn, string sortType)
        {
            return new List<TypeSortDescriptor>
                    {
                        new TypeSortDescriptor {Member = sortColumn,Direction = sortType},
                    };
        }

        protected List<TypeFilterDescriptor> CreateFilteringParameters(string filterColumn)
        {
            var filteringParameters = new List<TypeFilterDescriptor>();

            if (!string.IsNullOrEmpty(filterColumn))
            {
                if (filterColumn.Contains("~or~"))
                {
                    SplitOrOperator(filterColumn, filteringParameters);
                }
                else
                {
                    SplitAndOperator(filterColumn, filteringParameters);
                }
            }
            return filteringParameters;
        }

        protected static void SplitOrOperator(string filterColumn, List<TypeFilterDescriptor> filteringParameters)
        {
            var filterSplit = filterColumn.Split(new[] { "~or~" }, StringSplitOptions.RemoveEmptyEntries);

            var grades = new StringBuilder();

            for (int i = 0; i < filterSplit.Count(); i++)
            {
                var columnFilter = filterSplit[i].Split('~')[0];
                var valueFilter = filterSplit[i].Split('~')[2].Replace("'", "");
                var operatorFilter = filterSplit[i].Split('~')[1];

                filteringParameters.Add(new TypeFilterDescriptor
                {
                    Member = columnFilter,
                    Value = grades.ToString(),
                    Operator = operatorFilter
                });
            }
        }

        protected void SplitAndOperator(string filterColumn, List<TypeFilterDescriptor> filteringParameters)
        {
            var filterSplit = filterColumn.Split(new[] { "~and~" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < filterSplit.Count(); i++)
            {
                var columnFilter = filterSplit[i].Split('~')[0];
                var operatorFilter = filterSplit[i].Split('~')[1];
                var valueFilter = filterSplit[i].Split('~')[2].Replace("'", "");

                filteringParameters.Add(new TypeFilterDescriptor
                {
                    Member = columnFilter,
                    Value = valueFilter,
                    Operator = operatorFilter
                });
            }
        }

        public JsonResult GetDataForDropdown(int listCategoryTypeId)
        {
            try
            {
                _logger.Trace("Start BaseController - GetDataForDropdown: " + DateTime.Now);

                if (listCategoryTypeId == 0)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Param },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var response = _listCategoryService.GetDataForDropdown(listCategoryTypeId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetDataForDropdown: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ListCategoryEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetListCategoryType()
        {
            try
            {
                _logger.Trace("Start BaseController - GetListCategoryType: " + DateTime.Now);
                var response = _listCategoryTypeService.GetListCategoryType();
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetListCategoryType: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ListCategoryTypeEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllNationality()
        {
            try
            {
                _logger.Trace("Start BaseController - GetAllNationality: " + DateTime.Now);
                var response = _listCategoryService.GetAllNationality();
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetAllNationality: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new NationalityEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllProvinceCity(int nationalityId)
        {
            try
            {
                _logger.Trace("Start BaseController - GetAllProvinceCity: " + DateTime.Now);

                var response = _listCategoryService.GetAllProvinceCity(nationalityId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetAllProvinceCity: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ProvinceCityEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllDistrict(int provinceCityId)
        {
            try
            {
                _logger.Trace("Start BaseController - GetAllDistrict: " + DateTime.Now);

                var response = _listCategoryService.GetAllDistrict(provinceCityId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetAllDistrict: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new DistrictEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAllWards(int districtId)
        {
            try
            {
                _logger.Trace("Start BaseController - GetAllWards: " + DateTime.Now);

                var response = _listCategoryService.GetAllWards(districtId);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetAllWards: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new WardsEntity(), JsonRequestBehavior.AllowGet);
            }
        }

        public ExcelWorksheet GenerateExcelWorksheetListCategoryType(ExcelFile excelFile, int exportType)
        {
            var data = new List<Dictionary<string, object>>();
            var result = _listCategoryTypeService.GetListCategoryType();
            if (result != null && result.Data != null)
            {
                data = DictionaryHelper.DictionaryToList(result.Data);
            }

            var header = new List<ExportToExcelColumns>();
            header.AddRange(new List<ExportToExcelColumns> {
            new ExportToExcelColumns{DisplayOrder = 1,TitleColumn = Constants.ExportExcelTitleColumn.Id,NameColumn = Constants.ExportExcelColumn.Id},
            new ExportToExcelColumns{DisplayOrder = 2,TitleColumn = Constants.ExportExcelTitleColumn.Code,NameColumn = Constants.ExportExcelColumn.Code},
            new ExportToExcelColumns{DisplayOrder = 3,TitleColumn = Constants.ExportExcelTitleColumn.Name,NameColumn = Constants.ExportExcelColumn.Name}
            });
            var ws = new GenerationWorksheet().CreateWorksheet(excelFile, new CommonExcel() { ColumnsList = header, DataList = data }, Constants.SheetName.ListCategoryType, exportType);
            return ws;
        }

        public JsonResult GetStatusForDropdown(string type)
        {
            try
            {
                _logger.Trace("Start BaseController - GetStatusForDropdown: " + DateTime.Now);

                if (type == string.Empty || type == null)
                {
                    return new JsonResult()
                    {
                        Data = new { status = HttpStatusCode.NotFound, message = Constants.MessageInformation.Cannot_Find_Any_With_Given_Param },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }

                var response = _listCategoryService.GetStatusForDropdown(type);
                if (!response.Success)
                {
                    return Json(null);
                }
                var data = response.Data;
                _logger.Info("BaseController - Param: " + JsonConvert.SerializeObject(data));
                _logger.Trace("End BaseController - GetStatusForDropdown: " + DateTime.Now);
                return new JsonResult()
                {
                    Data = data,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return Json(new ListStatusEntity(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}