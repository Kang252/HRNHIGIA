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
    public class AttachmentInformationService : IAttachmentInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly AttachmentInformationRepository _repository = new AttachmentInformationRepository();

        public Response<AttachmentInformationEntity> DownloadAttachmentInformation(int id)
        {
            try
            {
                var result = _repository.DownloadAttachmentInformation(id);
                if (result != null)
                {
                    return result;
                }
                return new Response<AttachmentInformationEntity>(false, "", null);
            }
            catch (Exception e)
            {
                _logger.Error($"DownloadFileAttachment<{typeof(string).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<AttachmentInformationEntity>(false, e.Message, null);
            }
        }

        public ResponseList<AttachmentInformationEntity> GetAllAttachmentInformation(int id, int employeeId, int employeesOnBusinessTripId)
        {
            _logger.Trace("Start AttachmentInformationService - GetAllAttachmentInformation: " + DateTime.Now);
            var result = _repository.GetAllAttachmentInformation(id, employeeId, employeesOnBusinessTripId);
            _logger.Info("AttachmentInformationService - GetAllAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End AttachmentInformationService - GetAllAttachmentInformation: " + DateTime.Now);
            return new ResponseList<AttachmentInformationEntity>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<AttachmentInformationEntity> SaveAttachmentInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start AttachmentInformationService - SaveAttachmentInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<AttachmentInformationEntity>(content);
                var data = MapperHelper.Map<AttachmentInformationEntity, TypeAttachmentInformation>(saveData);
                var result = _repository.SaveAttachmentInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("AttachmentInformationService - SaveAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End AttachmentInformationService - SaveAttachmentInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("AttachmentInformationService - SaveAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End AttachmentInformationService - SaveAttachmentInformation: " + DateTime.Now);
                return new Response<AttachmentInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<AttachmentInformationEntity>(false, e.Message, null);
            }
        }

        public Response<AttachmentViewModel> UpdateAttachmentInformation(string content)
        {
            try
            {
                _logger.Trace("Start AttachmentInformationService - UpdateAttachmentInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<AttachmentViewModel>(content);
                var data = MapperHelper.Map<AttachmentViewModel, TypeAttachment>(saveData);
                var result = _repository.UpdateAttachmentInformation(data);
                if (result != null)
                {
                    _logger.Info("AttachmentInformationService - UpdateAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End AttachmentInformationService - UpdateAttachmentInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("AttachmentInformationService - UpdateAttachmentInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End AttachmentInformationService - UpdateAttachmentInformation: " + DateTime.Now);
                return new Response<AttachmentViewModel>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<AttachmentViewModel>(false, e.Message, null);
            }
        }
    }
}
