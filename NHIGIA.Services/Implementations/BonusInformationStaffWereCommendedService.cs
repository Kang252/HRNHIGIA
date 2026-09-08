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
    public class BonusInformationStaffWereCommendedService : IBonusInformationStaffWereCommendedService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly BonusInformationStaffWereCommendedRepository _repository = new BonusInformationStaffWereCommendedRepository();

        public ResponseList<BonusInformationStaffWereCommendedViewModel> GetAllBonusInformationStaffWereCommended(int bonusInformationId)
        {
            _logger.Trace("Start BonusInformationStaffWereCommendedService - GetAllBonusInformationStaffWereCommended: " + DateTime.Now);
            var result = _repository.GetAllBonusInformationStaffWereCommended(bonusInformationId);
            _logger.Info("BonusInformationStaffWereCommendedService - GetAllBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End BonusInformationStaffWereCommendedService - GetAllBonusInformationStaffWereCommended: " + DateTime.Now);
            return new ResponseList<BonusInformationStaffWereCommendedViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public string SaveBonusInformationStaffWereCommended(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<BonusInformationStaffWereCommendedViewModel>(content);
                var listData = new List<BonusInformationStaffWereCommendedEntity>();
                // for action add
                if (saveData.ListEmployeeId != null && saveData.ListEmployeeId.Count > 0)
                {
                    foreach (var item in saveData.ListEmployeeId)
                    {
                        var listItem = new BonusInformationStaffWereCommendedEntity()
                        {
                            BonusInformationId = item.BonusInformationId,
                            EmployeeId = item.EmployeeId
                        };
                        listData.Add(listItem);
                    }
                    var data = listData.ConvertToCustomUserDefinedDataTable();
                    var result = _repository.SaveBonusInformationStaffWereCommended(data, isAction);
                    if (result != null)
                    {
                        _logger.Info("BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
                        _logger.Trace("End BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
                        return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                    }
                    _logger.Info("BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
                }
                else
                {
                    // for action delete
                    if (isAction == (int)ActionCode.Delete)
                    {
                        var listItem = new BonusInformationStaffWereCommendedEntity()
                        {
                            Id = saveData.Id
                        };
                        listData.Add(listItem);
                        var data = listData.ConvertToCustomUserDefinedDataTable();
                        var result = _repository.SaveBonusInformationStaffWereCommended(data, isAction);
                        if (result != null)
                        {
                            _logger.Info("BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
                            _logger.Trace("End BonusInformationStaffWereCommendedService - SaveBonusInformationStaffWereCommended: " + DateTime.Now);
                            return JsonConvert.SerializeObject(Constants.MessageInformation.Success);
                        }
                    }
                }
                return JsonConvert.SerializeObject(new BonusInformationStaffWereCommendedEntity());
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return JsonConvert.SerializeObject(Constants.MessageInformation.Failed);
            }
        }

        public Response<BonusInformationStaffWereCommendedEntity> SaveOnlyBonusInformationStaffWereCommended(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start BonusInformationStaffWereCommendedService - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<BonusInformationStaffWereCommendedEntity>(content);
                var data = MapperHelper.Map<BonusInformationStaffWereCommendedEntity, TypeBonusInformationStaffWereCommended>(saveData);
                var result = _repository.SaveOnlyBonusInformationStaffWereCommended(data, isAction);
                if (result != null)
                {
                    _logger.Info("BonusInformationStaffWereCommendedService - SaveOnlyBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End BonusInformationStaffWereCommendedService - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
                    return result;
                }
                _logger.Info("BonusInformationStaffWereCommendedService - SaveOnlyBonusInformationStaffWereCommended - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End BonusInformationStaffWereCommendedService - SaveOnlyBonusInformationStaffWereCommended: " + DateTime.Now);
                return new Response<BonusInformationStaffWereCommendedEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<BonusInformationStaffWereCommendedEntity>(false, e.Message, null);
            }
        }
    }
}
