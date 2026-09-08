using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.TypedEntities;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Infrastructure
{
    public interface IEmployeeInformationRepository
    {
        ResponseList<EmployeeInformationViewModel> GetAllEmployeeInformation(PagingData param);
        Response<ProfileEntity> SaveProfile(TypeProfile param, int isAction);
        Response<ProfileEntity> GetEmployeeInformationById(int id);
        ResponseList<EmployeeInformationViewModel> GetEmployeeForAutoCompleBox(string keyword);
        ResponseList<EmployeeInformationViewModel> GetEmployee(PagingData param, int id, string type);
    }
}
