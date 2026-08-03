using DevJobParser.Infrastructure.HtmlExtractors;

namespace DevJobParser.Infrastructure.Fields
{
    public abstract class AbstractHtmlField : AbstractField
    {
        public required IHtmlDataExtractor Strategy { get; init; } = null!;
    }
}