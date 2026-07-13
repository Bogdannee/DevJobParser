using AngleSharp.Dom;

namespace DevJobParser.Infrastructure.HtmlExtractors
{
    public class HtmlTagAttributeExtractor : IHtmlDataExtractor
    {
        private readonly string _attributeName;
        private readonly string _prefix;

        public HtmlTagAttributeExtractor(string attributeName)
        {
            _attributeName = attributeName;
            _prefix = string.Empty;
        }

        public HtmlTagAttributeExtractor(string attributeName, string prefix) : this(attributeName)
        {
            _prefix = prefix;
        }

        public string? RetrieveData(IElement element)
        {
            string? data = element?.GetAttribute(_attributeName);
            if(data is null)
                return null;

            if(_prefix != string.Empty)
                    data = _prefix + data;

            return data;
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