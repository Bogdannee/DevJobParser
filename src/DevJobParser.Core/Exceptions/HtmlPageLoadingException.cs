namespace DevJobParser.Core.Exceptions
{
    public class HtmlPageLoadingException : Exception
    {
        public string Url { get; }

        public HtmlPageLoadingException(string url) : base()
        {
            Url = url;
        }

        public HtmlPageLoadingException(string url, string? message) : base(message)
        {
            Url = url;
        }

        public HtmlPageLoadingException(string url, string? message, Exception? innerException) : base(message, innerException)
        {
            Url = url;
        }
    }
}