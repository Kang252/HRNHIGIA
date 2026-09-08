namespace NHIGIA.Core.Domain.TypedEntities
{
    public class TypeAttachment : IUserDefinedType
    {
        public int? EmployeeId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileContent { get; set; }
    }
}
