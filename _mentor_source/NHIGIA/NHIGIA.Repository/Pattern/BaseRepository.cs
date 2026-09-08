using Dapper;
using DapperExtensions;
using NHIGIA.Common.Constants;
using NHIGIA.Core.Helper;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace NHIGIA.Repository.Pattern
{
    public class BaseRepository<TEntityType> : IBaseRepository<TEntityType>
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected readonly Microsoft.Practices.EnterpriseLibrary.Data.Database Database;
        protected readonly IDbConnection _db;

        public BaseRepository()
        {
            var factory = new DatabaseProviderFactory();
            Database = factory.Create(Constants.MainConnectionString);
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[Constants.MainConnectionString].ConnectionString;
            _db = new SqlConnection(connectionString);
        }

        #region Private Methods

        private static IEnumerable<PropertyInfo> GetAllProperties<T>()
        {
            return typeof(T).GetProperties().Where(info => info.GetMethod != null && info.GetMethod.IsPublic
                    && info.SetMethod != null && info.SetMethod.IsPublic
                    && !Attribute.IsDefined(info, typeof(NotMappedAttribute))).ToList();
        }

        private TStoredProcedureType MapRow<TStoredProcedureType>(IDataReader reader)
        {
            var entity = (TStoredProcedureType)Activator.CreateInstance(typeof(TStoredProcedureType));
            //Get all columns from reader 
            var columnNames = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            foreach (var propertyInfo in GetAllProperties<TStoredProcedureType>())
            {
                if (columnNames.Any(t => string.Equals(t, propertyInfo.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (reader[propertyInfo.Name] is DBNull) continue;

                    var propertyType = propertyInfo.PropertyType;
                    if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                    {
                        propertyInfo.SetValue(entity, bool.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(byte) || propertyType == typeof(byte?))
                    {
                        propertyInfo.SetValue(entity, byte.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                    {
                        propertyInfo.SetValue(entity, DateTime.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                    {
                        propertyInfo.SetValue(entity, decimal.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(double) || propertyType == typeof(double?))
                    {
                        propertyInfo.SetValue(entity, double.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(float) || propertyType == typeof(float?))
                    {
                        propertyInfo.SetValue(entity, float.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                    {
                        propertyInfo.SetValue(entity, Guid.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(short) || propertyType == typeof(short?))
                    {
                        propertyInfo.SetValue(entity, short.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(int) || propertyType == typeof(int?))
                    {
                        propertyInfo.SetValue(entity, int.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(long) || propertyType == typeof(long?))
                    {
                        propertyInfo.SetValue(entity, long.Parse(reader[propertyInfo.Name].ToString()), null);
                    }
                    else if (propertyType == typeof(string))
                    {
                        propertyInfo.SetValue(entity, reader[propertyInfo.Name].ToString(), null);
                    }
                    else if (propertyType == typeof(byte[]) || propertyType == typeof(byte?[]))
                    {
                        propertyInfo.SetValue(entity, (byte[])reader[propertyInfo.Name], null);
                    }
                }
            }
            return entity;
        }

        private void PrepareParameters(DbCommand sqlCmd, params StoredProcedureParameter[] parameters)
        {
            if (parameters == null) return;
            foreach (var para in parameters)
            {
                switch (para.Direction)
                {
                    case ParameterDirection.Input:
                        if (para.DbType == DbType.Object)
                            sqlCmd.Parameters.Add(new SqlParameter($"@{para.Name}", SqlDbType.Structured) { Value = para.Value });
                        else
                            Database.AddInParameter(sqlCmd, para.Name, para.DbType, para.Value);
                        break;
                    case ParameterDirection.Output:
                        Database.AddOutParameter(sqlCmd, para.Name, para.DbType, para.Size);
                        break;
                }
            }
        }

        #endregion

        public ResponseList<TEntityType> ListByStoredProcedure(string storedName, object param)
        {
            try
            {
                var result = _db.Query<TEntityType>(storedName, param, commandType: CommandType.StoredProcedure).ToList();
                _logger.Info($"ListByStoredProcedure<{typeof(TEntityType).Name}> result count: {result.Count()}");
                return new ResponseList<TEntityType>(true, Constants.MessageInformation.Success, result, result.Count);
            }
            catch (Exception e)
            {
                _logger.Error($"ListByStoredProcedure<{typeof(TEntityType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new ResponseList<TEntityType>(false, Constants.MessageInformation.Error, null, 0);
            }
        }

        public ResponseList<TEntityType> ListByStoredProcedure(string storedName, params StoredProcedureParameter[] parameters)
        {
            return ListByStoredProcedure<TEntityType>(storedName, parameters);
        }

        public Response<TEntityType> GetByStoredProcedure(string storedName, params StoredProcedureParameter[] parameters)
        {
            return GetByStoredProcedure<TEntityType>(storedName, parameters);
        }

        public ResponseList<TStoredProcedureType> ListByStoredProcedure<TStoredProcedureType>(string storedName, params StoredProcedureParameter[] parameters)
        {
            try
            {
                var result = new List<TStoredProcedureType>();
                using (var sqlCmd = Database.GetStoredProcCommand(storedName))
                {
                    PrepareParameters(sqlCmd, parameters);
                    using (var reader = Database.ExecuteReader(sqlCmd))
                    {
                        while (reader.Read())
                        {
                            result.Add(MapRow<TStoredProcedureType>(reader));
                        }
                        reader.Close();
                        foreach (var parameter in parameters.Where(t => t.Direction == ParameterDirection.Output))
                        {
                            parameter.Value = Database.GetParameterValue(sqlCmd, $"@{parameter.Name}");
                        }
                    }
                }
                return new ResponseList<TStoredProcedureType>(true, string.Empty, result, result.Count);
            }
            catch (Exception e)
            {
                _logger.Error($"ListByStoredProcedure<{typeof(TStoredProcedureType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new ResponseList<TStoredProcedureType>(false, "CommonError", null, 0);
            }
        }

        public Response<TStoredProcedureType> GetByStoredProcedure<TStoredProcedureType>(string storedName, params StoredProcedureParameter[] parameters)
        {
            try
            {
                var result = default(TStoredProcedureType);
                using (var sqlCmd = Database.GetStoredProcCommand(storedName))
                {
                    PrepareParameters(sqlCmd, parameters);
                    using (var reader = Database.ExecuteReader(sqlCmd))
                    {
                        if (reader.Read())
                        {
                            result = MapRow<TStoredProcedureType>(reader);
                        }
                        reader.Close();
                        foreach (var parameter in parameters.Where(t => t.Direction == ParameterDirection.Output))
                            parameter.Value = Database.GetParameterValue(sqlCmd, parameter.Name);
                    }
                }
                _logger.Info($"GetByStoredProcedure<{typeof(TStoredProcedureType).Name}> result: {(result == null ? "NULL" : result.ToString())}");

                return new Response<TStoredProcedureType>(true, null, result);
            }
            catch (Exception e)
            {
                _logger.Error($"GetByStoredProcedure<{typeof(TStoredProcedureType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<TStoredProcedureType>(false, "CommonError", default(TStoredProcedureType));
            }
        }

        public Response<DataTable> GetBySQLDynamic(string sql)
        {
            try
            {
                DataTable result = new DataTable();
                using (var sqlCmd = Database.GetSqlStringCommand(sql))
                {
                    using (var reader = Database.ExecuteReader(sqlCmd))
                    {
                        result.Load(reader);
                        reader.Close();
                    }
                }
                _logger.Info($"GetBySQLDynamic<{typeof(DataTable).Name}> result: {(result == null ? "NULL" : result.Rows.Count.ToString())}");

                return new Response<DataTable>(true, null, result);
            }
            catch (Exception e)
            {
                _logger.Error($"GetBySQLDynamic<{typeof(DataTable).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<DataTable>(false, "CommonError", default(DataTable));
            }
        }

        public Response<DataSet> GetByMultipleSQLDynamic(string sql)
        {
            try
            {
                DataSet result = null;
                using (var sqlCmd = Database.GetSqlStringCommand(sql))
                {
                    result = Database.ExecuteDataSet(sqlCmd);

                }
                _logger.Info($"GetBySQLDynamic<{typeof(DataSet).Name}> result: {(result == null ? "NULL" : "")}");

                return new Response<DataSet>(true, null, result);
            }
            catch (Exception e)
            {
                _logger.Error($"GetBySQLDynamic<{typeof(DataSet).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<DataSet>(false, "CommonError", default(DataSet));
            }
        }

        public Response CallStoredProcedure(string storedName, params StoredProcedureParameter[] parameters)
        {
            try
            {
                using (var sqlCmd = Database.GetStoredProcCommand(storedName))
                {
                    PrepareParameters(sqlCmd, parameters);
                    var result = Database.ExecuteNonQuery(sqlCmd);

                    foreach (var parameter in parameters.Where(t => t.Direction == ParameterDirection.Output))
                        parameter.Value = Database.GetParameterValue(sqlCmd, parameter.Name);

                    _logger.Info($"CallStoredProcedure result count: {result}");
                }
                return new Response(true, null);
            }
            catch (Exception e)
            {
                _logger.Error($"CallStoredProcedure exception: {e.Message}\n {e.StackTrace}");
                return new Response(false, "CommonError");
            }
        }
    }
}
