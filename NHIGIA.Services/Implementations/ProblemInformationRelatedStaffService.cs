using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;

namespace NHIGIA.Services.Implementations
{
    public class ProblemInformationRelatedStaffService : IProblemInformationRelatedStaffService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ProblemInformationRelatedStaffRepository _repository = new ProblemInformationRelatedStaffRepository();

        public ResponseList<ProblemInformationRelatedStaffViewModel> GetAllProblemInformationRelatedStaff(int id, int problemInformationId)
        {
            _logger.Trace("Start ProblemInformationRelatedStaffService - GetAllProblemInformationRelatedStaff: " + DateTime.Now);
            var result = _repository.GetAllProblemInformationRelatedStaff(id, problemInformationId);
            _logger.Info("ProblemInformationRelatedStaffService - GetAllProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End ProblemInformationRelatedStaffService - GetAllProblemInformationRelatedStaff: " + DateTime.Now);
            return new ResponseList<ProblemInformationRelatedStaffViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public string SaveProblemInformationRelatedStaff(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ProblemInformationRelatedStaffViewModel>(content);
                var listData = new List<ProblemInformationRelatedStaffEntity>();
                // for action add
                if (saveData.ListEmployeeId != null && saveData.ListEmployeeId.Count > 0)
                {
                    foreach (var item in saveData.ListEmployeeId)
                    {
                        var listItem = new ProblemInformationRelatedStaffEntity()
                        {
                            ProblemInformationId = item.ProblemInformationId,
                            EmployeeId = item.EmployeeId
                        };
                        listData.Add(listItem);
                    }
                    var data = listData.ConvertToCustomUserDefinedDataTable();
                    var result = _repository.SaveProblemInformationRelatedStaff(data, isAction);
                    if (result != null)
                    {
                        _logger.Info("ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
                        _logger.Trace("End ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff: " + DateTime.Now);
                        return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                    }
                    _logger.Info("ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff: " + DateTime.Now);
                }
                else
                {
                    // for action delete
                    if (isAction == (int)ActionCode.Delete)
                    {
                        var listItem = new ProblemInformationRelatedStaffEntity()
                        {
                            Id = saveData.Id
                        };
                        listData.Add(listItem);
                        var data = listData.ConvertToCustomUserDefinedDataTable();
                        var result = _repository.SaveProblemInformationRelatedStaff(data, isAction);
                        if (result != null)
                        {
                            _logger.Info("ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
                            _logger.Trace("End ProblemInformationRelatedStaffService - SaveProblemInformationRelatedStaff: " + DateTime.Now);
                            return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                        }
                    }
                }
                return JsonConvert.SerializeObject(new ProblemInformationRelatedStaffEntity());
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return JsonConvert.SerializeObject(Constants.MessageInformation.Failed);
            }
        }

        public Response<ProblemInformationRelatedStaffEntity> SaveOnlyProblemInformationRelatedStaff(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start ProblemInformationRelatedStaffService - SaveOnlyProblemInformationRelatedStaff: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ProblemInformationRelatedStaffEntity>(content);
                var data = MapperHelper.Map<ProblemInformationRelatedStaffEntity, TypeProblemInformationRelatedStaff>(saveData);
                var result = _repository.SaveOnlyProblemInformationRelatedStaff(data, isAction);
                if (result != null)
                {
                    _logger.Info("ProblemInformationRelatedStaffService - SaveOnlyProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End ProblemInformationRelatedStaffService - SaveOnlyProblemInformationRelatedStaff: " + DateTime.Now);
                    return result;
                }
                _logger.Info("ProblemInformationRelatedStaffService - SaveOnlyProblemInformationRelatedStaff - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End ProblemInformationRelatedStaffService - SaveOnlyProblemInformationRelatedStaff: " + DateTime.Now);
                return new Response<ProblemInformationRelatedStaffEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<ProblemInformationRelatedStaffEntity>(false, e.Message, null);
            }
        }
    }
}
