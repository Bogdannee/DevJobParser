using Microsoft.Extensions.Logging;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using AngleSharp.Html.Parser;

namespace DevJobParser.Parsers.WorkUa.LinkParser
{
    public class WorkUaJobLinkParser
    {
        private readonly ILogger<WorkUaJobLinkParser> _logger;
        private readonly IHtmlLoader _htmlLoader;
        private readonly Dictionary<string, ParsingRule> _jobLinkSelector;
        private readonly HtmlParser _htmlParser;

        public WorkUaJobLinkParser(ILogger<WorkUaJobLinkParser> logger, IHtmlLoader workUaHtmlLoader, HtmlParser htmlParser)
        {
            _logger = logger;
            _htmlLoader = workUaHtmlLoader;
            _htmlParser = htmlParser;
            _jobLinkSelector = new Dictionary<string, ParsingRule>()
            {
                {
                    "JobLink",
                    new ParsingRule
                    {
                        Selector = "div#pjax-jobs-list > div.card h2 > a",
                        Strategy = new HtmlTagAttributeExtractor(attributeName:"href", prefix:"https://www.work.ua")
                    }
                }
            };
        }

        public async Task<List<string>> GetJobLinksFromSearchLink(string searchLink, int maxPages, CancellationToken cancellationToken)
        {
            var parsedJobLinkList = new List<string>();

            for (int currentPageNumber = 1; currentPageNumber <= maxPages; currentPageNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<string>? jobLinkListOnCurrentPage;

                try
                {
                    var currentPageUrl = BuildUrlWithPage(searchLink, currentPageNumber);
                    jobLinkListOnCurrentPage = await GetJobLinkListOnPage(currentPageUrl, cancellationToken);
                }
                catch (HtmlPageLoadingException ex)
                {
                    _logger.LogError(ex, "Pagination stopped due to page loading error {Page}", currentPageNumber);
                    break;
                }

                _logger.LogInformation($"Parsed page: {currentPageNumber}");

                if (jobLinkListOnCurrentPage is null)
                {
                    break;
                }

                parsedJobLinkList.AddRange(jobLinkListOnCurrentPage);

                await Task.Delay(500, cancellationToken);
            }

            return parsedJobLinkList;
        }

        private async Task<List<string>?> GetJobLinkListOnPage(string searchLink, CancellationToken cancellationToken)
        {
            var htmlPage = await _htmlLoader.GetHtmlAsync(searchLink, cancellationToken);

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
}