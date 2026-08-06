using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Infrastructure.Fields
{
    public class JsonField(JobFieldName name) : AbstractField
    {
        public override JobFieldName Name { get; } = name;
    }
}
