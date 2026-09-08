namespace NHIGIA.Core.Domain.DtoEntities
{
    public class RequestDto
    {

    }

    public class RequestDto<T> : RequestDto //where T : INHIGIADto
    {
        public T Data { get; set; }
    }
}
