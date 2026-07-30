using Microsoft.Extensions.Logging;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;

namespace DevJobParser.Parsers.WorkUa.LinkParser
{
    public class WorkUaJobLinkParser
    {
        private readonly ILogger<WorkUaJobLinkParser> _logger;
        private readonly IHttpContentLoader _httpContentLoader;
        private readonly HtmlField _jobUrlField;
        private readonly HtmlExtractController _htmlExtractController;

        public WorkUaJobLinkParser(
            ILogger<WorkUaJobLinkParser> logger,
            IHttpContentLoader httpContentLoader,
            HtmlExtractController htmlExtractController)
        {
            _logger = logger;
            _httpContentLoader = httpContentLoader;
            _htmlExtractController = htmlExtractController;
            _jobUrlField = new HtmlField()
            {
                Name = JobFieldName.Url,
                Selector = "div#pjax-jobs-list > div.card h2 > a",
                Strategy = new HtmlTagAttributeExtractor(attributeName:"href", prefix:"https://www.work.ua"),
                Quantity = ValueQuantity.One
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

                _logger.LogInformation("Parsed page: {currentPageNumber}", currentPageNumber);

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
            var htmlPage = await _httpContentLoader.GetHtmlAsync(searchLink, cancellationToken);

            _htmlExtractController.ParseDocument(htmlPage);
            var jobLinks = _htmlExtractController.GetListElements(_jobUrlField);

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