namespace DevJobParser.Parsers.WorkUa.Options
{
    public class WorkUaParserOptions
    {
        public string SourceName {get; set;} = string.Empty;
        public string SearchLink { get; set; } = string.Empty;
        public int MaxPages { get; set; }
    }
}