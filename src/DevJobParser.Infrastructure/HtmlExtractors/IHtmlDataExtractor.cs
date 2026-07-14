using AngleSharp.Dom;

namespace DevJobParser.Infrastructure.HtmlExtractors
{
    public interface IHtmlDataExtractor
    {
        string? RetrieveData(IElement element);
        List<string?>? RetrieveData(IHtmlCollection<IElement> elements);
    }
}