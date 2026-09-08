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
    public class DegreeInformationService : IDegreeInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly DegreeInformationRepository _repository = new DegreeInformationRepository();

        public ResponseList<DegreeInformationViewModel> GetAllDegreeInformation(int id, int employeeId)
        {
            _logger.Trace("Start DegreeInformationService - GetAllDegreeInformation: " + DateTime.Now);
            var result = _repository.GetAllDegreeInformation(id, employeeId);
            _logger.Info("DegreeInformationService - GetAllDegreeInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End DegreeInformationService - GetAllDegreeInformation: " + DateTime.Now);
            return new ResponseList<DegreeInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<DegreeInformationEntity> SaveDegreeInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start DegreeInformationService - SaveDegreeInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<DegreeInformationEntity>(content);
                var data = MapperHelper.Map<DegreeInformationEntity, TypeDegreeInformation>(saveData);
                var result = _repository.SaveDegreeInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("DegreeInformationService - SaveDegreeInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End DegreeInformationService - SaveDegreeInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("DegreeInformationService - SaveDegreeInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End DegreeInformationService - SaveDegreeInformation: " + DateTime.Now);
                return new Response<DegreeInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<DegreeInformationEntity>(false, e.Message, null);
            }
        }
    }
}
