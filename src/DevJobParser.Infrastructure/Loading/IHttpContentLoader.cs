namespace DevJobParser.Infrastructure.Loading
{
    public interface IHttpContentLoader
    {
        Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken);
    }
}
