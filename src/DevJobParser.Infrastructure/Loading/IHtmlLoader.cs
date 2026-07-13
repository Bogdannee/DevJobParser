namespace DevJobParser.Infrastructure.Loading
{
    public interface IHtmlLoader
    {
        Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken);
    }
}
