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
    public class PageInformationService : IPageInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly PageInformationRepository _repository = new PageInformationRepository();

        public ResponseList<PageInformationViewModel> GetAllPageInformation(int id, int employeeId)
        {
            _logger.Trace("Start PageInformationService - GetAllPageInformation: " + DateTime.Now);
            var result = _repository.GetAllPageInformation(id, employeeId);
            _logger.Info("PageInformationService - GetAllPageInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End PageInformationService - GetAllPageInformation: " + DateTime.Now);
            return new ResponseList<PageInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<PageInformationEntity> SavePageInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start PageInformationService - SavePageInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<PageInformationEntity>(content);
                var data = MapperHelper.Map<PageInformationEntity, TypePageInformation>(saveData);
                var result = _repository.SavePageInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("PageInformationService - SavePageInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End PageInformationService - SavePageInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("PageInformationService - SavePageInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End PageInformationService - SavePageInformation: " + DateTime.Now);
                return new Response<PageInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<PageInformationEntity>(false, e.Message, null);
            }
        }
    }
}
