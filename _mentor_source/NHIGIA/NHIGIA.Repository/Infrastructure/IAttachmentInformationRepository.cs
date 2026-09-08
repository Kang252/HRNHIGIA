using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IAttachmentInformationRepository
    {
        ResponseList<AttachmentInformationEntity> GetAllAttachmentInformation(int id, int employeeId, int employeesOnBusinessTripId);
        Response<AttachmentInformationEntity> SaveAttachmentInformation(TypeAttachmentInformation param, int isAction);
        Response<AttachmentInformationEntity> DownloadAttachmentInformation(int id);
        Response<AttachmentViewModel> UpdateAttachmentInformation(TypeAttachment param);
    }
}
