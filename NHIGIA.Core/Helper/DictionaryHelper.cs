using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NHIGIA.Core.Helper
{
    public static class DictionaryHelper
    {
        public static string ToQueryString(this Dictionary<string, string> dict)
        {
            var list = new List<string>();
            foreach (var item in dict)
            {
                list.Add(item.Key + "=" + item.Value);
            }
            return string.Join("&", list);
        }

        public static List<Dictionary<string, object>> DictionaryToList<T>(List<T> data) where T : class
        {
            return data.Select(x => x.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                          .ToDictionary(prop => prop.Name, prop => prop.GetValue(x, null))).ToList();
        }
    }
}
