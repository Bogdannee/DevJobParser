using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Infrastructure.Fields
{
    public class HtmlAdditionalField : AbstractHtmlField
    {
        public override JobFieldName Name { get; } = JobFieldName.AdditionalDetails;
        public required string AdditionalDetailName {get; init;} = string.Empty;
    }
}