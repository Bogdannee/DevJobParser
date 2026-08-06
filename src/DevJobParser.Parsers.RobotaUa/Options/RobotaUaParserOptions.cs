namespace DevJobParser.Parsers.RobotaUa.Options
{
    public class RobotaUaParserOptions
    {
        public string SourceName {get; set;} = string.Empty;
        public string SearchLink { get; set; } = string.Empty;
        public int MaxPages { get; set; }
    }
}