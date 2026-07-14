namespace DevJobParser.Infrastructure.HtmlExtractors
{
    public class ParsingRule
    {
        public string Selector { get; init; } = string.Empty;
        public IHtmlDataExtractor Strategy { get; init; } = null!;
    }
}