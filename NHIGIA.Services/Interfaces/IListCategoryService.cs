using NHIGIA.Core.Domain.Entity;
using NHIGIA.Core.Domain.ViewModel;
using NHIGIA.Core.Helper;

namespace NHIGIA.Services.Interfaces
{
    public interface IListCategoryService
    {
        ResponseList<ListCategoryViewModel> GetAllListCategory(PagingData param);
        Response<ListCategoryEntity> GetListCategoryById(int id);
        Response<ListCategoryEntity> SaveListCategory(string content, int isAction);
        Response<CheckDuplicateViewModel> CheckDuplicate(int id, string tableName, string name, int? listCategoryTypeId);
        ResponseList<ListCategoryEntity> GetDataForDropdown(int listCategoryTypeId);
        ResponseList<NationalityEntity> GetAllNationality();
        ResponseList<ProvinceCityEntity> GetAllProvinceCity(int nationalityId);
        ResponseList<DistrictEntity> GetAllDistrict(int provinceCityId);
        ResponseList<WardsEntity> GetAllWards(int districtId);
        ResponseList<ListStatusEntity> GetStatusForDropdown(string type);

    }
}
