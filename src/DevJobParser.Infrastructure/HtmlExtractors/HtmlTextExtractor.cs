using AngleSharp.Dom;

namespace DevJobParser.Infrastructure.HtmlExtractors
{
    public class HtmlTextExtractor : IHtmlDataExtractor
    {
        public string? RetrieveData(IElement element)
        {
            if(element == null)
            {
                return null;
            }

            var cleanedText = element.Text().Trim();

            return string.IsNullOrWhiteSpace(cleanedText) ? null : cleanedText;
        }
        
        public List<string?>? RetrieveData(IHtmlCollection<IElement> elements)
        {
            if (elements is null || elements.Length == 0)
                return null;

            List<string?>? result = new List<string?>();

            foreach (var element in elements)
            {
                var data = RetrieveData(element);
                result.Add(data);
            }

            return result;
        }
    }
}