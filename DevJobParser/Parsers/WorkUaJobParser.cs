using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DevJobParser.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace DevJobParser.Parsers
{
    public interface IHtmlTagGetData
    {
        string? RetrieveData(IElement element);
        List<string?>? RetrieveData(IHtmlCollection<IElement> elements);
    }

    public class HtmlTagGetText : IHtmlTagGetData
    {
        public string? RetrieveData(IElement element)
        {
            if(element == null)
            {
                return null;
            }

            var cleanedText = element.Text().Trim();

            return string.IsNullOrWhiteSpace(cleanedText) ? null : cleanedText;
        }

        public List<string?>? RetrieveData(IHtmlCollection<IElement> elements)
        {
            if (elements is null || elements.Length == 0)
                return null;

            List<string?>? result = new List<string?>();

            foreach (var element in elements)
            {
                var data = RetrieveData(element);
                result.Add(data);
            }

            return result;
        }
    }

    public class HtmlTagGetAttribute : IHtmlTagGetData
    {
        private readonly string _attributeName;
        private readonly string _prefix;

        public HtmlTagGetAttribute(string attributeName)
        {
            _attributeName = attributeName;
            _prefix = string.Empty;
        }

        public HtmlTagGetAttribute(string attributeName, string prefix) : this(attributeName)
        {
            _prefix = prefix;
        }

        public string? RetrieveData(IElement element)
        {
            string? data = element?.GetAttribute(_attributeName);
            if(data is null)
                return null;

            if(_prefix != string.Empty)
                    data = _prefix + data;

            return data;
        }

        public List<string?>? RetrieveData(IHtmlCollection<IElement> elements)
        {
            if (elements is null || elements.Length == 0)
                return null;
            
            List<string?>? result = new List<string?>();

            foreach (var element in elements)
            {
                var data = RetrieveData(element);
                result.Add(data);
            }

            return result;
        }
    }

    public class ParsingRule
    {
        public string Selector { get; init; } = string.Empty;
        public IHtmlTagGetData Strategy { get; init; } = null!;
    }

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

    public class WorkUaHtmlLoader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WorkUaHtmlLoader> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        public WorkUaHtmlLoader(HttpClient httpClient, ILogger<WorkUaHtmlLoader> logger)
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

    public class WorkUaJobLinkParser
    {
        private readonly ILogger<WorkUaJobLinkParser> _logger;
        private readonly WorkUaHtmlLoader _workUaHtmlLoader;
        private readonly Dictionary<string, ParsingRule> _jobLinkSelector;
        private readonly HtmlParser _htmlParser;

        public WorkUaJobLinkParser(ILogger<WorkUaJobLinkParser> logger, WorkUaHtmlLoader workUaHtmlLoader, HtmlParser htmlParser)
        {
            _logger = logger;
            _workUaHtmlLoader = workUaHtmlLoader;
            _htmlParser = htmlParser;
            _jobLinkSelector = new Dictionary<string, ParsingRule>()
            {
                {
                    "JobLink",
                    new ParsingRule
                    {
                        Selector = "div#pjax-jobs-list > div.card h2 > a",
                        Strategy = new HtmlTagGetAttribute(attributeName:"href", prefix:"https://www.work.ua")
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
            var htmlPage = await _workUaHtmlLoader.GetHtmlAsync(searchLink, cancellationToken);

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

    public class WorkUaJobDetailsParser : IDisposable
    {
        private readonly ILogger<WorkUaJobDetailsParser> _logger;
        private readonly Dictionary<string, ParsingRule> _jobDetailSelectors;
        private readonly SemaphoreSlim _throttler;
        private readonly WorkUaHtmlLoader _workUaHtmlLoader;
        private readonly HtmlParser _htmlParser;


        public WorkUaJobDetailsParser(WorkUaHtmlLoader workUaHtmlLoader, ILogger<WorkUaJobDetailsParser> logger, HtmlParser htmlParser)
        {
            _workUaHtmlLoader = workUaHtmlLoader;
            _logger = logger;
            _htmlParser = htmlParser;
            _throttler = new SemaphoreSlim(initialCount: 5);

            _jobDetailSelectors = new Dictionary<string, ParsingRule>()
            {
                {
                    "jobTitle",
                    new ParsingRule
                    {
                        Selector = "div.card h1",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobCompanyName",
                    new ParsingRule
                    {
                        Selector = "div.card ul li span.glyphicon-company + a span",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobSalary",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > div.row + ul > li span[title=\"Зарплата\"] + span",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobDescription",
                    new ParsingRule
                    {
                        Selector = "div.card div.company-description",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobPlaceOfWork",
                    new ParsingRule
                    {
                        Selector =
                        "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Місце роботи\"])",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobTermsAndConditions",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Умови й вимоги\"])",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobLanguageKnowladge",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Знання мов\"])",
                        Strategy = new HtmlTagGetText()
                    }
                },
                {
                    "jobTagsOfSkillsCollection",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > ul + div > ul > li > span",
                        Strategy = new HtmlTagGetText()
                    }
                },
            };
        }

        public void Dispose()
        {
            _throttler.Dispose();
        }

        public async Task<List<JobCard>> GetJobDetailsList(List<string> parsedJobLinkList, CancellationToken cancellationToken)
        {
            var parsingTasks = parsedJobLinkList.Select(async parsedJobLink =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _throttler.WaitAsync(cancellationToken);

                try
                {
                string htmlPage = await _workUaHtmlLoader.GetHtmlAsync(parsedJobLink, cancellationToken);

                var angleHtmlDocument = _htmlParser.ParseDocument(htmlPage);

                // Main fields
                string? jobTitle = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobTitle"]);
                string? jobCompanyName = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobCompanyName"]);
                string? jobSalary = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobSalary"]);
                string? jobDescription = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobDescription"]);

                if (jobTitle is null || jobCompanyName is null || jobDescription is null)
                {
                    throw new JobParsingException(parsedJobLink, "One of a main fields is null.");
                }

                // Additional fields
                string? jobPlaceOfWork = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobPlaceOfWork"]);
                string? jobTermsAndConditions = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobTermsAndConditions"]);
                string? jobLanguageKnowladge = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobLanguageKnowladge"]);

                List<string?>? jobTagsOfSkillsCollection = GetDataFromHtmlTags(angleHtmlDocument, _jobDetailSelectors["jobTagsOfSkillsCollection"]);
                string? jobTagsOfSkills = GetStringFromTextList(jobTagsOfSkillsCollection);

                var additionalDetails = new Dictionary<string, string?>()
                {
                    { "placeOfWork", jobPlaceOfWork },
                    { "termsAndConditions", jobTermsAndConditions },
                    { "languageKnowladge", jobLanguageKnowladge },
                    { "tagsOfSkills", jobTagsOfSkills }
                };

                return new JobCard()
                {
                    Url = parsedJobLink,
                    Title = jobTitle,
                    Company = jobCompanyName,
                    Salary = jobSalary,
                    Description = jobDescription,
                    AdditionalDetails = additionalDetails,
                };

            }
            catch(HtmlPageLoadingException ex)
            {
                _logger.LogError(ex, "Failed to retrieve or parse job details after several retries: {ParsedJobLink}", parsedJobLink);
                return null;
            }
            catch (JobParsingException ex)
            {
                _logger.LogWarning(ex, "A vacancy with incomplete data was missed: {ParsedJobLink}", parsedJobLink);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while parsing job details for link: {ParsedJobLink}", parsedJobLink);
                return null;
            }
            finally
            {
                _throttler.Release();
            }

            }).ToList();

            var jobCards = await Task.WhenAll(parsingTasks);

            return jobCards.Where(card => card != null).ToList();
        }

        private string? GetDataFromHtmlTag(IHtmlDocument angleHtmlDocument, ParsingRule parsingRule)
        {
            var htmlElement = angleHtmlDocument?.QuerySelector(parsingRule.Selector);

            if (htmlElement is null)
                return null;

            var data = parsingRule.Strategy.RetrieveData(htmlElement);

            return data;
        }

        private List<string?>? GetDataFromHtmlTags(IHtmlDocument angleHtmlDocument, ParsingRule parsingRule)
        {
            var htmlElementCollection = angleHtmlDocument?.QuerySelectorAll(parsingRule.Selector);

            if (htmlElementCollection is null)
                return null;

            var dataList = parsingRule.Strategy.RetrieveData(htmlElementCollection);

            return dataList;
        }

        private string? GetStringFromTextList(List<string?>? textCollection)
        {
            if (textCollection is null || textCollection.Count == 0 )
            {
                return null;
            }

            return string.Join(", ",textCollection);
        }
    }
    public class WorkUaJobParser
    {
        private readonly WorkUaJobLinkParser _workUaJobLinkParser;
        private readonly WorkUaJobDetailsParser _workUaJobDetailsParser;
        private readonly string _searchlink;
        private readonly int _maxPages;


        public WorkUaJobParser(
            WorkUaJobLinkParser workUaJobLinkParser,
            WorkUaJobDetailsParser workUaJobDetailsParser,
            IOptions<WorkUaParserOptions> options)
        {
            _workUaJobLinkParser = workUaJobLinkParser;
            _workUaJobDetailsParser = workUaJobDetailsParser;
            _searchlink = options.Value.SearchLink;
            _maxPages = options.Value.MaxPages;
        }

        public async Task<List<JobCard>> GetJobCardList(CancellationToken cancellationToken)
        {
            List<string> parsedJobLinkList = await _workUaJobLinkParser.GetJobLinksFromSearchLink(_searchlink, _maxPages, cancellationToken);
            List<JobCard> parsedJobDetailsList = await _workUaJobDetailsParser.GetJobDetailsList(parsedJobLinkList, cancellationToken);

            return parsedJobDetailsList;
        }
    }

    public class WorkUaParserOptions
    {
        public string SearchLink { get; set; } = string.Empty;
        public int MaxPages { get; set; } = 50;
    }
}