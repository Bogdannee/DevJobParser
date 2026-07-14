using Microsoft.Extensions.Logging;
using DevJobParser.Core.DTO;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

namespace DevJobParser.Parsers.WorkUa.DetailsParser
{
    public class WorkUaJobDetailsParser : IDisposable
    {
        private readonly ILogger<WorkUaJobDetailsParser> _logger;
        private readonly Dictionary<string, ParsingRule> _jobDetailSelectors;
        private readonly SemaphoreSlim _throttler;
        private readonly IHtmlLoader _workUaHtmlLoader;
        private readonly HtmlParser _htmlParser;


        public WorkUaJobDetailsParser(IHtmlLoader workUaHtmlLoader, ILogger<WorkUaJobDetailsParser> logger, HtmlParser htmlParser)
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
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobCompanyName",
                    new ParsingRule
                    {
                        Selector = "div.card ul li span.glyphicon-company + a span",
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobSalary",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > div.row + ul > li span[title=\"Зарплата\"] + span",
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobDescription",
                    new ParsingRule
                    {
                        Selector = "div.card div.company-description",
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobPlaceOfWork",
                    new ParsingRule
                    {
                        Selector =
                        "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Місце роботи\"])",
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobTermsAndConditions",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Умови й вимоги\"])",
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobLanguageKnowladge",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Знання мов\"])",
                        Strategy = new HtmlTextExtractor()
                    }
                },
                {
                    "jobTagsOfSkillsCollection",
                    new ParsingRule
                    {
                        Selector = "div.card div.wordwrap > ul + div > ul > li > span",
                        Strategy = new HtmlTextExtractor()
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
}