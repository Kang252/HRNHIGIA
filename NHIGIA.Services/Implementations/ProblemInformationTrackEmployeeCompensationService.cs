using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
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
    public class ProblemInformationTrackEmployeeCompensationService : IProblemInformationTrackEmployeeCompensationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ProblemInformationTrackEmployeeCompensationRepository _repository = new ProblemInformationTrackEmployeeCompensationRepository();

        public ResponseList<ProblemInformationTrackEmployeeCompensationViewModel> GetAllProblemInformationTrackEmployeeCompensation(int problemInformationId, int employeeId, int type)
        {
            _logger.Trace("StartProblemInformationTrackEmployeeCompensationService - GetAllProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
            var result = _repository.GetAllProblemInformationTrackEmployeeCompensation(problemInformationId, employeeId, type);
            _logger.Info("ProblemInformationTrackEmployeeCompensationService - GetAllProblemInformationTrackEmployeeCompensation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("EndProblemInformationTrackEmployeeCompensationService - GetAllProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
            return new ResponseList<ProblemInformationTrackEmployeeCompensationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public string SaveProblemInformationTrackEmployeeCompensation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start EndProblemInformationTrackEmployeeCompensationService - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<ProblemInformationTrackEmployeeCompensationViewModel>(content);
                var listData = new List<ProblemInformationTrackEmployeeCompensationEntity>();
                // for action add
                if (saveData.ListProblemInformationTrackEmployeeCompensation != null && saveData.ListProblemInformationTrackEmployeeCompensation.Count > 0)
                {
                    foreach (var item in saveData.ListProblemInformationTrackEmployeeCompensation)
                    {
                        var listItem = new ProblemInformationTrackEmployeeCompensationEntity()
                        {
                            Id = item.Id,
                            ProblemInformationId = item.ProblemInformationId,
                            EmployeeId = item.EmployeeId,
                            AmountMoney = item.AmountMoney,
                            PayDay = item.PayDay,
                            SourceCompensation = item.SourceCompensation,
                            Type = item.Type,
                            CreatedBy = item.CreatedBy,
                            ModifiedBy = item.ModifiedBy,
                            IsDeleted = item.IsDeleted
                        };
                        listData.Add(listItem);
                    }
                    var data = listData.ConvertToCustomUserDefinedDataTable();
                    var result = _repository.SaveProblemInformationTrackEmployeeCompensation(data, isAction);
                    if (result != null)
                    {
                        _logger.Info("EndProblemInformationTrackEmployeeCompensationService - SaveProblemInformationTrackEmployeeCompensation - Data: " + JsonConvert.SerializeObject(result));
                        _logger.Trace("End EndProblemInformationTrackEmployeeCompensationService - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
                        return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                    }
                    _logger.Info("EndProblemInformationTrackEmployeeCompensationService - SaveProblemInformationTrackEmployeeCompensation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End EndProblemInformationTrackEmployeeCompensationService - SaveProblemInformationTrackEmployeeCompensation: " + DateTime.Now);
                }
                return JsonConvert.SerializeObject(new ProblemInformationTrackEmployeeCompensationEntity());
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return JsonConvert.SerializeObject(Constants.MessageInformation.Failed);
            }
        }
    }
}
