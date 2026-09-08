using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace NHIGIA.Core.Helper
{
    public static class DataTableHelper
    {
        public static DataTable ConvertToDataTable<T>(this IEnumerable<T> values)
        {
            var table = new DataTable();
            table.Columns.Add("Item", typeof(T));
            if (values != null)
            {
                foreach (var value in values)
                {
                    table.Rows.Add(value);
                }
            }
            return table;
        }

        public static DataTable ConvertToUserDefinedDataTable<T>(this IEnumerable<T> values) where T : class
        {
            var table = new DataTable();

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        var newRow = table.NewRow();
                        foreach (var prop in properties)
                        {
                            if (table.Columns.Contains(prop.Name))
                                newRow[prop.Name] = prop.GetValue(value, null) ?? DBNull.Value;
                        }
                        table.Rows.Add(newRow);
                    }
                }
            }
            return table;
        }
        public static bool IsGenericList(this object o)
        {
            var oType = o.GetType();
            return (oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
        }

        public static DataTable ConvertToCustomUserDefinedDataTable<T>(this IEnumerable<T> values) where T : class
        {
            var table = new DataTable();

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                if (!Attribute.IsDefined(prop, typeof(NotMappedAttribute)))
                {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }

            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        var newRow = table.NewRow();
                        foreach (var prop in properties)
                        {
                            if (table.Columns.Contains(prop.Name))
                                newRow[prop.Name] = prop.GetValue(value, null) ?? DBNull.Value;
                        }
                        table.Rows.Add(newRow);
                    }
                }
            }
            return table;
        }

        public static DataTable ConvertToExcelDataTable<T>(this IEnumerable<T> values) where T : class
        {
            var table = new DataTable();

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var propName = prop.Name;
                var attrs = prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    propName = ((DescriptionAttribute)attrs[0]).Description;
                }
                table.Columns.Add(propName, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        var newRow = table.NewRow();
                        foreach (var prop in properties)
                        {
                            var propName = prop.Name;
                            var attrs = prop.GetCustomAttributes(typeof(DescriptionAttribute), false);
                            if (attrs.Length > 0)
                            {
                                propName = ((DescriptionAttribute)attrs[0]).Description;
                            }

                            if (table.Columns.Contains(propName))
                                newRow[propName] = prop.GetValue(value, null) ?? DBNull.Value;
                        }
                        table.Rows.Add(newRow);
                    }
                }
            }
            return table;
        }

    }
}
