using NHIGIA.Core.Helper;

namespace NHIGIA.Repository.Pattern
{
    public interface ICommonRepository<TEType, in TKType>
    {
        ResponseList<TEType> GetAll();
        ResponseList<TEType> GetActive();
        Response<TEType> FindById(TKType id);
        Response<TEType> Add(TEType obj);
        Response Update(TEType obj);
        Response Remove(TKType id);
        ResponseList<TEType> ListByStoredProcedure(string storedName, object param);
        Response<TEType> GetByStoredProcedure(string storedName, object param);
        Response GetResultExecuteStoredProcedure(string storedName, object param);
        Response<T> GetScalarResultFromStoredProcedure<T>(string storedName, object param);
        Response ExecuteStoredProcedure(string storedName, object param);
    }
}
