using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface ICertificateInformationService
    {
        ResponseList<CertificateInformationViewModel> GetAllCertificateInformation(int id, int employeeId);
        Response<CertificateInformationEntity> SaveCertificateInformation(string content, int isAction);
    }
}
