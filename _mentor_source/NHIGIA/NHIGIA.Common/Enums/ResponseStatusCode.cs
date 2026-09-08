namespace NHIGIA.Common.Enums
{
    public enum ResponseStatusCode
    {
        Fail = 0,
        Success = 1,
        NotFound = 2,
        Duplicated = 3
    }

    public enum ActionCode
    {
        CreateUpdate = 1,
        Delete = 2
    }

    public enum ExportType
    {
        None = 0,
        ExportForExcel = 1,
        ExportForImport = 2
    }
}
