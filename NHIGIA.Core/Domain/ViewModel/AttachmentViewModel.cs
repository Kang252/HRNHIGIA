namespace NHIGIA.Core.Domain.ViewModel
{
    public class AttachmentViewModel
    {
        public int? EmployeeId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileContent { get; set; }
    }
}
