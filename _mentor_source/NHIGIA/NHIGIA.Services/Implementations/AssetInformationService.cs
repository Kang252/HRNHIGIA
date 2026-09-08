using NHIGIA.Common.Constants;
using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;
using NHIGIA.Repository.Repositories;
using NHIGIA.Services.Interfaces;
using Newtonsoft.Json;
using NLog;
using System;

namespace NHIGIA.Services.Implementations
{
    public class AssetInformationService : IAssetInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly AssetInformationRepository _repository = new AssetInformationRepository();

        public ResponseList<AssetInformationViewModel> GetAllAssetInformation(int id, int employeeId)
        {
            _logger.Trace("Start AssetInformationService - GetAllAssetInformation: " + DateTime.Now);
            var result = _repository.GetAllAssetInformation(id, employeeId);
            _logger.Info("AssetInformationService - GetAllAssetInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AssetInformationService - GetAllAssetInformation: " + DateTime.Now);
            return new ResponseList<AssetInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<AssetInformationEntity> SaveAssetInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start AssetInformationService - SaveAssetInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<AssetInformationEntity>(content);
                var data = MapperHelper.Map<AssetInformationEntity, TypeAssetInformation>(saveData);
                var result = _repository.SaveAssetInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("AssetInformationService - SaveAssetInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End AssetInformationService - SaveAssetInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("AssetInformationService - SaveAssetInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End AssetInformationService - SaveAssetInformation: " + DateTime.Now);
                return new Response<AssetInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<AssetInformationEntity>(false, e.Message, null);
            }
        }
    }
}
