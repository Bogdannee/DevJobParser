using Microsoft.Extensions.Logging;
using Polly.Retry;
using Polly;
using DevJobParser.Core.Exceptions;

namespace DevJobParser.Infrastructure.Loading
{
    public class HtmlLoader : IHtmlLoader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HtmlLoader> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        public HtmlLoader(HttpClient httpClient, ILogger<HtmlLoader> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _retryPolicy = Policy.Handle<HttpRequestException>(ex =>
                ex.StatusCode == null ||
                (int)ex.StatusCode >= 500 ||
                ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests
            ).WaitAndRetryAsync(
                retryCount:3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        $"Retry {retryCount} for URL: {context["Url"]}. Delaying for {timeSpan.TotalSeconds} seconds. Error: {exception.Message}"
                    );
                }
            );
        }

        public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken)
        {
            string htmlPage;
            try
            {
                htmlPage = await _retryPolicy.ExecuteAsync(async (context, ct) =>
                {
                    context["Url"] = url;
                    return await _httpClient.GetStringAsync(url, ct);
                }, new Context(), cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new HtmlPageLoadingException(url, ex.Message, ex);
            }
            
            return htmlPage;
        }
    }
}