using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Infrastructure.Fields
{
    public abstract class AbstractField
    {
        public abstract JobFieldName Name { get; }
        public required string Selector { get; init; } = string.Empty;
        public required ValueQuantity Quantity { get; init; }
    }
}