using Dapper;
using NHIGIA.Common.Constants;
using NHIGIA.Core.Helper;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHIGIA.Repository.Pattern
{
    public class CommonRepository<TEType, TKType> : ICommonRepository<TEType, TKType>, IDisposable
    {
        protected readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected readonly IDbConnection _db;

        public CommonRepository()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[Constants.MainConnectionString].ConnectionString;
            _db = new SqlConnection(connectionString);
        }

        #region table name

        public static string TableName => LazyTableName.Value;

        private static readonly Lazy<string> LazyTableName = new Lazy<string>(() => typeof(TEType).Name);

        #endregion

        #region primary key

        public static string TableKey => LazyTableKey.Value;

        private static readonly Lazy<string> LazyTableKey = new Lazy<string>(() =>
        {
            var props = typeof(TEType).GetProperties();

            foreach (var prop in props)
            {
                var res = prop.GetCustomAttributes(typeof(KeyAttribute), false).FirstOrDefault();
                if (res != null) return prop.Name;
            }

            throw new InvalidOperationException("Model must have an property marked as [Key]");
        });

        #endregion

        #region columns list

        public static string ColumnList => LazyColumnList.Value;

        private static readonly Lazy<string> LazyColumnList = new Lazy<string>(() =>
        {
            var props = GetAllProperties();
            var result = new StringBuilder();

            foreach (var prop in props.Where(p => !Attribute.IsDefined(p, typeof(NotMappedAttribute))))
            {
                result.Append("[");
                result.Append(prop.Name);
                result.Append("]");
                result.Append(",");
            }
            result.Remove(result.Length - 1, 1);

            return result.ToString();
        });

        #endregion

        #region sql command text    

        protected static string SelectAllCommandText => $"SELECT {ColumnList} FROM dbo.[{TableName}]";
        protected static string SelectActiveCommandText => $"SELECT {ColumnList} FROM dbo.[{TableName}] WHERE IsDeleted = 0";
        protected static string SelectByIdCommandText => $"SELECT {ColumnList} FROM dbo.[{TableName}] WHERE [{TableKey}]=@Id";
        protected static string InsertCommandText => $"INSERT INTO dbo.[{TableName}] {InsertClause}";
        protected static string UpdateCommandText => $"UPDATE dbo.[{TableName}] SET {UpdateClause} WHERE [{TableKey}]=@{TableKey}";
        protected static string DeleteCommandText => $"UPDATE dbo.[{TableName}] SET IsDeleted = 1 WHERE [{TableKey}]=@Id";

        #endregion

        #region query clauses

        public static string InsertClause => LazyInsertClause.Value.Replace(",Function,", ",[Function],");
        private static readonly Lazy<string> LazyInsertClause = new Lazy<string>(() =>
        {
            var props = GetAllProperties();
            var generatedKey = false;
            var sbField = new StringBuilder("(");
            var sbValue = new StringBuilder("(");

            foreach (var prop in props)
            {
                if (prop.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), false).Any())
                {
                    generatedKey = true;
                    continue;
                }

                sbField.Append(prop.Name);
                sbField.Append(",");

                sbValue.Append("@");
                sbValue.Append(prop.Name);
                sbValue.Append(",");
            }
            sbField.Remove(sbField.Length - 1, 1);
            sbValue.Remove(sbValue.Length - 1, 1);
            sbField.Append(") VALUES ");
            sbValue.Append(")");
            sbField.Append(sbValue);
            sbValue.Append("; ");
            sbField.Append($"SELECT {ColumnList} FROM dbo.");
            sbField.Append(TableName);
            sbField.Append(" WHERE ");
            sbField.Append(TableKey);
            sbField.Append(" = ");
            sbField.Append(generatedKey ? "SCOPE_IDENTITY()" : $"@{TableKey}");

            return sbField.ToString();
        });

        public static string UpdateClause => LazyUpdateClause.Value.Replace(",Function,", ",[Function],");
        private static readonly Lazy<string> LazyUpdateClause = new Lazy<string>(() =>
        {
            var props = GetAllProperties();
            var sb = new StringBuilder();

            foreach (var prop in props)
            {
                if (prop.GetCustomAttributes(typeof(KeyAttribute), false).Any() ||
                    prop.GetCustomAttributes(typeof(DatabaseGeneratedAttribute), false).Any())
                    continue;

                sb.Append(prop.Name);
                sb.Append("=@");
                sb.Append(prop.Name);
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        });

        #endregion

        private static IEnumerable<PropertyInfo> GetAllProperties()
        {
            return typeof(TEType).GetProperties().Where(p => p.GetMethod.IsPublic && p.SetMethod.IsPublic && !Attribute.IsDefined(p, typeof(NotMappedAttribute)));
        }

        public virtual ResponseList<TEType> GetAll()
        {
            try
            {
                var result = _db.Query<TEType>(SelectAllCommandText).ToList();
                _logger.Info($"GetAll<{typeof(TEType).Name}> result count: {result.Count()}");
                return new ResponseList<TEType>(true, Constants.MessageInformation.Success, result, result.Count);
            }
            catch (Exception e)
            {
                _logger.Error($"GetAll<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new ResponseList<TEType>(false, Constants.MessageInformation.Failed, null, 0);
            }
        }

        public Response<TEType> FindById(TKType id)
        {
            try
            {
                var result = _db.Query<TEType>(SelectByIdCommandText, new { Id = id }).SingleOrDefault();
                _logger.Info($"FindById<{typeof(TEType).Name}> result: {(result == null ? "NULL" : result.ToString())}");
                return new Response<TEType>(true, Constants.MessageInformation.Success, result);
            }
            catch (Exception e)
            {
                _logger.Error($"FindById<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<TEType>(false, Constants.MessageInformation.Failed, default(TEType));
            }
        }

        public virtual Response<TEType> Add(TEType obj)
        {
            try
            {
                var result = _db.Query<TEType>(InsertCommandText, obj).Single();
                _logger.Info($"Add<{typeof(TEType).Name}> result: {(result == null ? "NULL" : result.ToString())}");
                return new Response<TEType>(true, Constants.MessageInformation.Success, result);
            }
            catch (Exception e)
            {
                _logger.Error($"Add<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<TEType>(false, Constants.MessageInformation.Failed, default(TEType));
            }
        }

        public Response Update(TEType obj)
        {
            try
            {
                var result = _db.Execute(UpdateCommandText, obj);
                _logger.Info($"Update<{typeof(TEType).Name}> result: {(result == 0 ? Constants.MessageInformation.Failed : Constants.MessageInformation.Success)}");
                return new Response(true, Constants.MessageInformation.Success);
            }
            catch (Exception e)
            {
                _logger.Error($"Update<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response(false, Constants.MessageInformation.Failed);
            }
        }

        public Response Remove(TKType id)
        {
            try
            {
                var result = _db.Execute(DeleteCommandText, new { Id = id });
                _logger.Info($"Remove<{typeof(TEType).Name}> result: {(result == 0 ? Constants.MessageInformation.Failed : Constants.MessageInformation.Success)}");
                return new Response(true, Constants.MessageInformation.Success);
            }
            catch (Exception e)
            {
                _logger.Error($"Remove<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response(false, Constants.MessageInformation.Failed);
            }
        }

        public ResponseList<TEType> ListByStoredProcedure(string storedName, object param)
        {
            try
            {
                var result = _db.Query<TEType>(storedName, param, commandType: CommandType.StoredProcedure).ToList();
                _logger.Info($"ListByStoredProcedure<{typeof(TEType).Name}> result count: {result.Count()}");
                return new ResponseList<TEType>(true, Constants.MessageInformation.Success, result, result.Count);
            }
            catch (Exception e)
            {
                _logger.Error($"ListByStoredProcedure<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new ResponseList<TEType>(false, Constants.MessageInformation.Failed, null, 0);
            }
        }

        public ResponseList<TEType> ListByStoredProcedureParam<TEType>(string storedName, params StoredProcedureParameter[] parameters)
        {
            try
            {

                var dynamicParameters = PrepareParameters(parameters);
                var result = _db.Query<TEType>(storedName, dynamicParameters, commandType: CommandType.StoredProcedure).ToList();

                foreach (var parameter in parameters)
                {
                    if (parameter.Direction == ParameterDirection.Output)
                    {
                        parameter.Value = dynamicParameters.Get<object>($"@{parameter.Name}");
                    }
                }
                _logger.Info($"ListByStoredProcedure<{typeof(TEType).Name}> result count: {result.Count}");
                return new ResponseList<TEType>(true, null, result, result.Count);
            }
            catch (Exception e)
            {
                _logger.Error($"ListByStoredProcedure<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new ResponseList<TEType>(false, Constants.MessageInformation.Failed, null, 0);
            }
        }

        private DynamicParameters PrepareParameters(params StoredProcedureParameter[] parameters)
        {
            var result = new DynamicParameters();
            foreach (var parameter in parameters)
            {
                result.Add($"@{parameter.Name}", parameter.Value, parameter.DbType, parameter.Direction);
            }
            return result;
        }

        public Response<TEType> GetByStoredProcedure(string storedName, object param)
        {
            try
            {
                var resultQuery = _db.Query<TEType>(storedName, param, commandType: CommandType.StoredProcedure);
                var eTypes = resultQuery as TEType[] ?? resultQuery.ToArray();
                var result = (resultQuery != null && eTypes.Any()) ? eTypes.Single() : default(TEType);
                _logger.Info($"GetByStoredProcedure<{typeof(TEType).Name}> result: {(result == null ? "NULL" : result.ToString())}");
                return new Response<TEType>(true, Constants.MessageInformation.Success, result);
            }
            catch (Exception e)
            {
                _logger.Error($"GetByStoredProcedure<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<TEType>(false, Constants.MessageInformation.Failed, default(TEType));
            }
        }

        public Response GetResultExecuteStoredProcedure(string storedName, object param)
        {
            try
            {
                var result = Convert.ToBoolean(_db.ExecuteScalar(storedName, param, commandType: CommandType.StoredProcedure));
                _logger.Info($"GetResultExecuteStoredProcedure<{storedName}> result: {result}");
                return new Response(result, Constants.MessageInformation.Success);
            }
            catch (Exception e)
            {
                _logger.Error($"GetResultExecuteStoredProcedure<{storedName}> exception: {e.Message}\n {e.StackTrace}");
                return new Response(false, Constants.MessageInformation.Failed);
            }
        }

        public Response<T> GetScalarResultFromStoredProcedure<T>(string storedName, object param)
        {
            try
            {
                var result = (T)(_db.ExecuteScalar(storedName, param, commandType: CommandType.StoredProcedure));
                _logger.Info($"GetScalarResultFromStoredProcedure<{storedName}> result: {result}");
                return new Response<T>(true, Constants.MessageInformation.Success, result);
            }
            catch (Exception e)
            {
                _logger.Error($"GetScalarResultFromStoredProcedure<{storedName}> exception: {e.Message}\n {e.StackTrace}");
                return new Response<T>(false, Constants.MessageInformation.Failed, default(T));
            }
        }

        public Response ExecuteStoredProcedure(string storedName, object param)
        {
            try
            {
                var result = _db.Execute(storedName, param, commandType: CommandType.StoredProcedure);
                _logger.Info($"ExecuteStoredProcedure<{storedName}> affected rows: {result}");
                return new Response(true, Constants.MessageInformation.Success);
            }
            catch (Exception e)
            {
                _logger.Error($"ExecuteStoredProcedure<{storedName}> exception: {e.Message}\n {e.StackTrace}");
                return new Response(false, Constants.MessageInformation.Failed);
            }
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public ResponseList<TEType> GetActive()
        {
            try
            {
                var result = _db.Query<TEType>(SelectActiveCommandText).ToList();
                _logger.Info($"GetAll<{typeof(TEType).Name}> result count: {result.Count()}");
                return new ResponseList<TEType>(true, Constants.MessageInformation.Success, result, result.Count);
            }
            catch (Exception e)
            {
                _logger.Error($"GetAll<{typeof(TEType).Name}> exception: {e.Message}\n {e.StackTrace}");
                return new ResponseList<TEType>(false, Constants.MessageInformation.Failed, null, 0);
            }
        }
    }
}
