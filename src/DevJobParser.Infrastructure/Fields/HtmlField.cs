using DevJobParser.Infrastructure.HtmlExtractors;

namespace DevJobParser.Infrastructure.Fields
{
    public class HtmlField : AbstractField
    {
        public required IHtmlDataExtractor Strategy { get; init; } = null!;
    }
}