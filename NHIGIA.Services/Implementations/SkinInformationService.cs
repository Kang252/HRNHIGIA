using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;

namespace NHIGIA.Services.Implementations
{
    public class SkinInformationService : ISkinInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SkinInformationRepository _repository = new SkinInformationRepository();

        public ResponseList<SkinInformationEntity> GetAllSkinInformation(int employeeId)
        {
            _logger.Trace("Start SkinInformationService - GetAllSkinInformation: " + DateTime.Now);
            var result = _repository.GetAllSkinInformation(employeeId);
            _logger.Info("SkinInformationService - GetAllSkinInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End SkinInformationService - GetAllSkinInformation: " + DateTime.Now);
            return new ResponseList<SkinInformationEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<SkinInformationEntity> SaveSkinInformation(string content)
        {
            try
            {
                _logger.Trace("Start SkinInformationService - SaveSkinInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<SkinInformationEntity>(content);
                var data = MapperHelper.Map<SkinInformationEntity, TypeSkinInformation>(saveData);
                var result = _repository.SaveSkinInformation(data);
                if (result != null)
                {
                    _logger.Info("SkinInformationService - SaveSkinInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End SkinInformationService - SaveSkinInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("SkinInformationService - SaveSkinInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End SkinInformationService - SaveSkinInformation: " + DateTime.Now);
                return new Response<SkinInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<SkinInformationEntity>(false, e.Message, null);
            }
        }
    }
}
