using GemBox.Spreadsheet;
using NHIGIA.Common.Constants;
using NHIGIA.Common.Enums;
using System.Linq;

namespace NHIGIA.Core.Helper
{
    public class GenerationWorksheet
    {
        public ExcelWorksheet CreateWorksheet(ExcelFile excelFile, CommonExcel data, string sheetName, int exportType)
        {
            var sheet = excelFile.Worksheets.Add(sheetName);

            var cell = sheet.Cells;
            var rowIndex = 0;
            var columIndex = 0;
            cell.Style.Font.Size = 11 * 20;

            sheet.Rows[0].Style.Font.Weight = ExcelFont.BoldWeight;
            sheet.Protected = true;
            sheet.ProtectionSettings.AllowSelectingLockedCells = false;
            sheet.ProtectionSettings.AllowInsertingRows = true;
            sheet.ProtectionSettings.AllowDeletingRows = true;
            sheet.ProtectionSettings.AllowInsertingColumns = true;
            sheet.ProtectionSettings.AllowUsingAutoFilter = true;

            if (data.ColumnsList != null)
            {
                foreach (var header in data.ColumnsList)
                {
                    cell[rowIndex, columIndex].Value = header.TitleColumn;
                    columIndex++;
                }
            }
            rowIndex++;
            columIndex = 0;
            if (data.DataList != null)
            {
                //find sheetname
                //find column
                //unlock
                switch (sheetName)
                {
                    case Constants.SheetName.ListCategory:
                        if (exportType == (int)ExportType.ExportForImport)
                        {
                            sheet.Columns[2].Style.Locked = false;
                            sheet.Columns[3].Style.Locked = false;
                            sheet.Columns[4].Style.Locked = false;
                            sheet.Rows[0].Style.Locked = true;

                            sheet.DataValidations.Add(new DataValidation(sheet.Columns[4].Cells)
                            {
                                Type = DataValidationType.List,
                                Formula1 = "=" + Constants.SheetName.ListCategoryType + "!$A$2:$A$" + (data.DataCountListCategoryType + 1).ToString(),
                                ErrorStyle = DataValidationErrorStyle.Stop
                            });
                        }
                        break;
                    default:
                        break;
                }

                var to = data.DataList.ToList();
                for (var n = 0; n < to.Count; n++)
                {
                    //Row index = 2
                    var item = to[n];
                    for (var i = 0; i < data.ColumnsList.Count; i++)
                    {
                        var colName = data.ColumnsList[i].NameColumn;
                        var value = item[colName] != null ? item[colName].ToString() : string.Empty;

                        cell[rowIndex, columIndex].Value = value;
                        columIndex++;
                    }
                    rowIndex++;
                    columIndex = 0;
                }

                int columnCount = sheet.CalculateMaxUsedColumns();
                for (int j = 0; j < columnCount; j++)
                {
                    sheet.Columns[j].AutoFit(1, sheet.Rows[0], sheet.Rows[sheet.Rows.Count - 1]);
                }

                var filterRange = sheet.Cells.GetSubrangeAbsolute(0, 0, rowIndex, data.ColumnsList.Count - 1);
                filterRange.Filter().Apply();
            }
            return sheet;
        }
    }
}
