using NHIGIA.Core.Helper;
using System.Data;

namespace NHIGIA.Repository.Pattern
{
    public interface IBaseRepository<TEntityType>
    {
        ResponseList<TEntityType> ListByStoredProcedure(string storedName, params StoredProcedureParameter[] parameters);
        Response<TEntityType> GetByStoredProcedure(string storedName, params StoredProcedureParameter[] parameters);
        ResponseList<TStoredProcedureType> ListByStoredProcedure<TStoredProcedureType>(string storedName, params StoredProcedureParameter[] parameters);
        Response<TStoredProcedureType> GetByStoredProcedure<TStoredProcedureType>(string storedName, params StoredProcedureParameter[] parameters);
        Response<DataTable> GetBySQLDynamic(string sql);
        Response<DataSet> GetByMultipleSQLDynamic(string sql);
        Response CallStoredProcedure(string storedName, params StoredProcedureParameter[] parameters);
    }
}
