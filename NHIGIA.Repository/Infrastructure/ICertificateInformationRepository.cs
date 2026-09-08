using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface ICertificateInformationRepository
    {
        ResponseList<CertificateInformationViewModel> GetAllCertificateInformation(int id, int employeeId);
        Response<CertificateInformationEntity> SaveCertificateInformation(TypeCertificateInformation param, int isAction);
    }
}
