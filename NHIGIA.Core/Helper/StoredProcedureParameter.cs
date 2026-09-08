using System.Data;

namespace NHIGIA.Core.Helper
{
    public class StoredProcedureParameter
    {
        public StoredProcedureParameter()
        {
        }

        public StoredProcedureParameter(string name, object value, DbType dbType) : this(name, value, dbType, ParameterDirection.Input, 0)
        {
        }

        public StoredProcedureParameter(string name, object value, DbType dbType, ParameterDirection direction)
        {
            Name = name;
            Value = value;
            DbType = dbType;
            Direction = direction;
        }

        public StoredProcedureParameter(string name, object value, DbType dbType, ParameterDirection direction, int size)
        {
            Name = name;
            Value = value;
            DbType = dbType;
            Direction = direction;
            Size = size;
        }

        public string Name { get; set; }
        public object Value { get; set; }
        public ParameterDirection Direction { get; set; }
        public DbType DbType { get; set; }
        public int Size { get; set; }
    }
}
