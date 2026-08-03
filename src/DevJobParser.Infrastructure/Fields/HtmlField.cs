using DevJobParser.Infrastructure.Fields.Enums;
using DevJobParser.Infrastructure.HtmlExtractors;

namespace DevJobParser.Infrastructure.Fields
{
    public class HtmlField(JobFieldName name) : AbstractHtmlField
    {
        public override JobFieldName Name { get; } = name;
    }
}