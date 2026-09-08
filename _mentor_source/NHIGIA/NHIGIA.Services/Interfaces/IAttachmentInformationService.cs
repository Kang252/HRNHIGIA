using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IAttachmentInformationService
    {
        ResponseList<AttachmentInformationEntity> GetAllAttachmentInformation(int id, int employeeId, int employeesOnBusinessTripId);
        Response<AttachmentInformationEntity> SaveAttachmentInformation(string content, int isAction);
        Response<AttachmentInformationEntity> DownloadAttachmentInformation(int id);
        Response<AttachmentViewModel> UpdateAttachmentInformation(string content);
    }
}
