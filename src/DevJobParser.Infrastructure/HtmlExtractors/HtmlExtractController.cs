using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Infrastructure.HtmlExtractors
{
    public class HtmlExtractController
    {
        private readonly HtmlParser _htmlParser;
        private IHtmlDocument _htmlDocument;

        public HtmlExtractController(HtmlParser htmlParser)
        {
            _htmlParser = htmlParser;
        }

        public void ParseDocument(string htmlPage)
        {
            _htmlDocument = _htmlParser.ParseDocument(htmlPage);
        }

        public string? GetStringifiedData(HtmlField htmlField)
        {
            string? result;

            if (htmlField.Quantity == ValueQuantity.One)
            {
                result = GetElement(htmlField);
            }
            else
            {
                var listElements = GetListElements(htmlField);
                result = GetStringifiedList(listElements);
            }

            return result;
        }
        private string? GetElement(HtmlField htmlField)
        {
            var htmlElement = _htmlDocument?.QuerySelector(htmlField.Selector);

            if (htmlElement is null)
                return null;

            var data = htmlField.Strategy.RetrieveData(htmlElement);

            return data;
        }

        public List<string?>? GetListElements(HtmlField htmlField)
        {
            var htmlElementCollection = _htmlDocument?.QuerySelectorAll(htmlField.Selector);

            if (htmlElementCollection is null)
                return null;

            var dataList = htmlField.Strategy.RetrieveData(htmlElementCollection);

            return dataList;
        }

        private string? GetStringifiedList(List<string?>? textCollection)
        {
            if (textCollection is null || textCollection.Count == 0 )
            {
                return null;
            }

            return string.Join(", ",textCollection);
        }
    }
}
