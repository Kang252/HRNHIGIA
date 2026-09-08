using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IEmployeeInformationService
    {
        ResponseList<EmployeeInformationViewModel> GetAllEmployeeInformation(PagingData param);
        Response<ProfileEntity> SaveProfile(string content, int isAction);
        Response<ProfileEntity> GetEmployeeInformationById(int id);
        ResponseList<EmployeeInformationViewModel> GetEmployeeForAutoCompleBox(string keyword);
        ResponseList<EmployeeInformationViewModel> GetEmployee(PagingData param, int id, string type);
    }
}
