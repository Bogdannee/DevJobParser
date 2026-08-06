using Microsoft.Extensions.Logging;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using AngleSharp.Html.Parser;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Parsers.Djinni.LinkParser;

public class DjinniJobLinkParser
{
    private readonly ILogger<DjinniJobLinkParser> _logger;
    private readonly IHttpContentLoader _httpContentLoader;
    private readonly HtmlField _jobUrlField;
    private readonly HtmlAdditionalField _jobCounter;
    private readonly HtmlExtractController _htmlExtractController;

    public DjinniJobLinkParser(
        ILogger<DjinniJobLinkParser> logger,
        IHttpContentLoader httpContentLoader,
        HtmlExtractController htmlExtractController)
    {
        _logger = logger;
        _httpContentLoader = httpContentLoader;
        _htmlExtractController = htmlExtractController;
        _jobUrlField = new HtmlField(JobFieldName.Url)
        {
            Selector = "div.job-item > div > a",
            Strategy = new HtmlTagAttributeExtractor(attributeName:"href", prefix:"https://djinni.co"),
            Quantity = ValueQuantity.Multiply
        };

        _jobCounter = new HtmlAdditionalField()
        {
            AdditionalDetailName = "JobCounter",
            Selector = "header > div > h1+span",
            Strategy = new HtmlTextExtractor(),
            Quantity = ValueQuantity.Single
        };
    }

    public async Task<List<string>> GetJobLinksFromSearchLink(string searchLink, int maxPages, CancellationToken cancellationToken)
    {
        var parsedJobLinkList = new List<string>();

        await LoadAndParseHtml(searchLink, cancellationToken);
        var actualJobCounter = _htmlExtractController.GetStringifiedField(_jobCounter);

        for (int currentPageNumber = 1; currentPageNumber <= maxPages; currentPageNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<string?>? jobLinkListOnCurrentPage;
            string? jobCounterOnPage;
            try
            {
                var currentPageUrl = BuildUrlWithPage(searchLink, currentPageNumber);
                await LoadAndParseHtml(currentPageUrl, cancellationToken);

                jobLinkListOnCurrentPage = _htmlExtractController.GetJobLinks(_jobUrlField);
                jobCounterOnPage = _htmlExtractController.GetStringifiedField(_jobCounter);
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

    private async Task LoadAndParseHtml(string url, CancellationToken cancellationToken)
    {
        var html = await _httpContentLoader.GetHtmlAsync(url, cancellationToken);
        _htmlExtractController.ParseDocument(html);
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
