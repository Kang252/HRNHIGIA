using System.Collections.Generic;

namespace NHIGIA.Core.Helper
{
    public class CommonExcel
    {
        public List<ExportToExcelColumns> ColumnsList { get; set; }
        public List<Dictionary<string, object>> DataList { get; set; }
        public int DataCountListCategory { get; set; }
        public int DataCountListCategoryType { get; set; }
    }
}
