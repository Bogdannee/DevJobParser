using Microsoft.Extensions.Logging;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using AngleSharp.Html.Parser;

namespace DevJobParser.Parsers.Djinni.LinkParser;

public class DjinniJobLinkParser
{
    private readonly ILogger<DjinniJobLinkParser> _logger;
    private readonly IHttpContentLoader _httpContentLoader;
    private readonly Dictionary<string, ParsingRule> _jobLinkSelector;
    private readonly HtmlParser _htmlParser;

    public DjinniJobLinkParser(ILogger<DjinniJobLinkParser> logger, IHttpContentLoader httpContentLoader, HtmlParser htmlParser)
    {
        _logger = logger;
            _httpContentLoader = httpContentLoader;
            _htmlParser = htmlParser;
            _jobLinkSelector = new Dictionary<string, ParsingRule>()
            {
                {
                    "JobLink",
                    new ParsingRule
                    {
                        Selector = "div.job-item > div > a",
                        Strategy = new HtmlTagAttributeExtractor(attributeName:"href", prefix:"https://djinni.co")
                    }
                },
                {
                    "JobCounter",
                    new ParsingRule
                    {
                        Selector = "header > div > h1+span",
                        Strategy = new HtmlTextExtractor()
                    }
                }
            };
    }

    public async Task<List<string>> GetJobLinksFromSearchLink(string searchLink, int maxPages, CancellationToken cancellationToken)
    {
        var parsedJobLinkList = new List<string>();
        var actualJobCounter = await GetJobCounterOnPage(searchLink, cancellationToken);

        for (int currentPageNumber = 1; currentPageNumber <= maxPages; currentPageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<string>? jobLinkListOnCurrentPage;
            string? jobCounterOnPage;
            try
            {
                var currentPageUrl = BuildUrlWithPage(searchLink, currentPageNumber);
                jobLinkListOnCurrentPage = await GetJobLinkListOnPage(currentPageUrl, cancellationToken);
                jobCounterOnPage = await GetJobCounterOnPage(currentPageUrl, cancellationToken);
            }
            catch (HtmlPageLoadingException ex)
            {
                _logger.LogError(ex, "Pagination stopped due to page loading error {Page}", currentPageNumber);
                break;
            }

            _logger.LogInformation("Parsed page: {currentPageNumber}", currentPageNumber);

            if (actualJobCounter != jobCounterOnPage || jobLinkListOnCurrentPage is null)
            {
                break;
            }

            parsedJobLinkList.AddRange(jobLinkListOnCurrentPage);

            await Task.Delay(500, cancellationToken);
        }

        return parsedJobLinkList;
    }

    public async Task<string?> GetJobCounterOnPage(string searchLink, CancellationToken cancellationToken)
    {
        var html = await _httpContentLoader.GetHtmlAsync(searchLink, cancellationToken);
        var angleDocument = _htmlParser.ParseDocument(html);

        if (!_jobLinkSelector.TryGetValue("JobCounter", out var linkRule))
        {
            throw new JobParsingException(searchLink, "Parsing rule for 'JobLink' is not configured in the dictionary!");
        }

        var counter = angleDocument.QuerySelector(linkRule.Selector);

        return linkRule.Strategy.RetrieveData(counter);
    }

    private async Task<List<string>?> GetJobLinkListOnPage(string searchLink, CancellationToken cancellationToken)
    {
        var htmlPage = await _httpContentLoader.GetHtmlAsync(searchLink, cancellationToken);

        var htmlDocumentObject = _htmlParser.ParseDocument(htmlPage);

        if (!_jobLinkSelector.TryGetValue("JobLink", out var linkRule))
        {
            throw new JobParsingException(searchLink, "Parsing rule for 'JobLink' is not configured in the dictionary!");
        }

        var htmlJobLinksOnPage = htmlDocumentObject.QuerySelectorAll(linkRule.Selector);

        if (htmlJobLinksOnPage.Length == 0)
        {
            return null;
        }

        var jobLinks = linkRule.Strategy.RetrieveData(htmlJobLinksOnPage);

        return jobLinks.Where(link => link is not null).Select(link => link!).ToList();
    }

    private string BuildUrlWithPage(string baseSearchLink, int pageNumber)
    {
        try
        {
            var uriBuilder = new UriBuilder(baseSearchLink);
            var queryParams = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            
            queryParams["page"] = pageNumber.ToString();
            uriBuilder.Query = queryParams.ToString();
            
            return uriBuilder.ToString();
        }
        catch (UriFormatException)
        {
            throw new ArgumentException($"Invalid base search link format: {baseSearchLink}");
        }
    }
}
