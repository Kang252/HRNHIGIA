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
    public class CertificateInformationService : ICertificateInformationService
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CertificateInformationRepository _repository = new CertificateInformationRepository();

        public ResponseList<CertificateInformationViewModel> GetAllCertificateInformation(int id, int employeeId)
        {
            _logger.Trace("Start CertificateInformationService - GetAllCertificateInformation: " + DateTime.Now);
            var result = _repository.GetAllCertificateInformation(id, employeeId);
            _logger.Info("CertificateInformationService - GetAllCertificateInformation - Data: " + JsonConvert.SerializeObject(result));
            _logger.Trace("End CertificateInformationService - GetAllCertificateInformation: " + DateTime.Now);
            return new ResponseList<CertificateInformationViewModel>(result.Success, result.Message, result.Data, result.Total);
        }

        public Response<CertificateInformationEntity> SaveCertificateInformation(string content, int isAction)
        {
            try
            {
                _logger.Trace("Start CertificateInformationService - SaveCertificateInformation: " + DateTime.Now);
                var saveData = JsonConvert.DeserializeObject<CertificateInformationEntity>(content);
                var data = MapperHelper.Map<CertificateInformationEntity, TypeCertificateInformation>(saveData);
                var result = _repository.SaveCertificateInformation(data, isAction);
                if (result != null)
                {
                    _logger.Info("CertificateInformationService - SaveCertificateInformation - Data: " + JsonConvert.SerializeObject(result));
                    _logger.Trace("End CertificateInformationService - SaveCertificateInformation: " + DateTime.Now);
                    return result;
                }
                _logger.Info("CertificateInformationService - SaveCertificateInformation - Data: " + JsonConvert.SerializeObject(result));
                _logger.Trace("End CertificateInformationService - SaveCertificateInformation: " + DateTime.Now);
                return new Response<CertificateInformationEntity>(result.Success, result.Message, result.Data);
            }
            catch (Exception e)
            {
                _logger.Error(Constants.MessageInformation.Failed + $": {e.Message}\n {e.StackTrace}");
                return new Response<CertificateInformationEntity>(false, e.Message, null);
            }
        }
    }
}
