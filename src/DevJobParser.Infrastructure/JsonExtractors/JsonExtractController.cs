using System.Text.Json.Nodes;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Infrastructure.JsonExtractors
{
    public class JsonExtractController
    {
        private JsonNode? _rootNode;

        public JsonExtractController()
        {
        }

        public bool ParseDocument(string jsonResponse)
        {
            throw new NotImplementedException();
        }

        public string? GetStringifiedField(JsonField jsonField)
        {
            string? result;

            if (jsonField.Quantity == ValueQuantity.Single)
            {
                result = GetElement(jsonField);
            }
            else
            {
                var listElements = GetListElements(jsonField);
                result = GetStringifiedList(listElements);
            }

            return result;
        }
        private string? GetElement(JsonField jsonField)
        {
            throw new NotImplementedException();
        }

        private List<string?>? GetListElements(JsonField jsonField)
        {
            throw new NotImplementedException();
        }

        public List<string?>? GetJobLinks(JsonField jsonField)
        {
            throw new NotImplementedException();
        }

        private string? GetStringifiedList(List<string?>? textCollection)
        {
            throw new NotImplementedException();
        }
    }
}
