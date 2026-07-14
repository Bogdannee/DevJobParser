namespace DevJobParser.Parsers.WorkUa
{
    public class WorkUaParserOptions
    {
        public string SourceName {get; set;} = string.Empty;
        public string SearchLink { get; set; } = string.Empty;
        public int MaxPages { get; set; } = 50;
    }
}