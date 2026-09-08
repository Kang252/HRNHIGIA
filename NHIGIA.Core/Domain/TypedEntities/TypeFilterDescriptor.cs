namespace NHIGIA.Core.Domain.TypedEntities
{
    public class TypeFilterDescriptor : IUserDefinedType
    {
        public string Member { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
    }
}
