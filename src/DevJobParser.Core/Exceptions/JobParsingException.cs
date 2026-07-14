namespace DevJobParser.Core.Exceptions
{
    public class JobParsingException : Exception
    {
        public string JobUrl { get; }

        public JobParsingException(string jobUrl) : base()
        {
            JobUrl = jobUrl;
        }

        public JobParsingException(string jobUrl, string? message) : base(message)
        {
            JobUrl = jobUrl;
        }

        public JobParsingException(string jobUrl, string? message, Exception? innerException) : base(message, innerException)
        {
            JobUrl = jobUrl;
        }
    }
}