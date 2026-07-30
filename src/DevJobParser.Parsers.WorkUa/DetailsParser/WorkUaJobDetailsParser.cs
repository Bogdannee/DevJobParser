using Microsoft.Extensions.Logging;
using DevJobParser.Core.DTO;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using DevJobParser.Infrastructure;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;
using DevJobParser.Infrastructure.Builders;

namespace DevJobParser.Parsers.WorkUa.DetailsParser
{
    public class WorkUaJobDetailsParser : IDisposable
    {
        private readonly ILogger<WorkUaJobDetailsParser> _logger;
        private readonly List<HtmlField> _jobDetailSelectors;
        private readonly SemaphoreSlim _throttler;
        private readonly IHttpContentLoader _httpContentLoader;
        private readonly HtmlExtractController _htmlExtractController;
        private readonly JobBuilder _JobBuilder;

        public WorkUaJobDetailsParser(
            HttpContentLoader httpContentLoader,
            ILogger<WorkUaJobDetailsParser> logger,
            HtmlExtractController htmlExtractController,
            JobBuilder jobBuilder)
        {
            _httpContentLoader = httpContentLoader;
            _logger = logger;
            _htmlExtractController = htmlExtractController;
            _JobBuilder = jobBuilder;
            _throttler = new SemaphoreSlim(initialCount: 5);

            _jobDetailSelectors = new List<HtmlField>()
            {
                new HtmlField()
                {
                    Name = JobFieldName.Title,
                    Selector = "div.card h1",
                    Strategy = new HtmlTextExtractor(),
                    Quantity = ValueQuantity.One
                },
                new HtmlField()
                {
                    Name = JobFieldName.Company,
                    Selector = "div.card ul li span.glyphicon-company + a span",
                    Strategy = new HtmlTextExtractor(),
                    Quantity = ValueQuantity.One
                },
                new HtmlField()
                {
                    Name = JobFieldName.Salary,
                    Selector = "div.card div.wordwrap > div.row + ul > li span[title=\"Зарплата\"] + span",
                    Strategy = new HtmlTextExtractor(),
                    Quantity = ValueQuantity.One
                },
                new HtmlField()
                {
                    Name = JobFieldName.Description,
                    Selector = "div.card div.company-description",
                    Strategy = new HtmlTextExtractor(),
                    Quantity = ValueQuantity.One
                },
                // new HtmlField()
                // {
                //     fieldName = JobFieldName.jobPlaceOfWork,
                //     Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Місце роботи\"])",
                //     Strategy = new HtmlTextExtractor()
                // },
                // new HtmlField()
                // {
                //     fieldName = JobFieldName.jobTermsAndConditions,
                //     Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Умови й вимоги\"])",
                //     Strategy = new HtmlTextExtractor()
                // },
                // new HtmlField()
                // {
                //     fieldName = JobFieldName.jobLanguageKnowladge,
                //     Selector = "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Знання мов\"])",
                //     Strategy = new HtmlTextExtractor()
                // },
                // new HtmlField()
                // {
                //     fieldName = JobFieldName.jobTagsOfSkillsCollection,
                //     Selector = "div.card div.wordwrap > ul + div > ul > li > span",
                //     Strategy = new HtmlTextExtractor()
                // },
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
                    var parsedFields = new Dictionary<JobFieldName, string?>();  
                    string htmlPage = await _httpContentLoader.GetHtmlAsync(parsedJobLink, cancellationToken);

                    _htmlExtractController.ParseDocument(htmlPage);

                    // Main fields
                    foreach (var field in _jobDetailSelectors)
                    {
                        string? parsedField = _htmlExtractController.GetStringifiedData(field);
                        parsedFields.Add(field.Name, parsedField);
                    }

                    // if (jobTitle is null || jobCompanyName is null || jobDescription is null)
                    // {
                    //     throw new JobParsingException(parsedJobLink, "One of a main fields is null.");
                    // }

                    // Additional fields
                    // string? jobPlaceOfWork = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobPlaceOfWork"]);
                    // string? jobTermsAndConditions = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobTermsAndConditions"]);
                    // string? jobLanguageKnowladge = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobLanguageKnowladge"]);

                    // List<string?>? jobTagsOfSkillsCollection = GetDataFromHtmlTags(angleHtmlDocument, _jobDetailSelectors["jobTagsOfSkillsCollection"]);
                    // string? jobTagsOfSkills = GetStringFromTextList(jobTagsOfSkillsCollection);

                    // var additionalDetails = new Dictionary<string, string?>()
                    // {
                    //     { "placeOfWork", jobPlaceOfWork },
                    //     { "termsAndConditions", jobTermsAndConditions },
                    //     { "languageKnowladge", jobLanguageKnowladge },
                    //     { "tagsOfSkills", jobTagsOfSkills }
                    // };

                    foreach (var parsedField in parsedFields)
                    {
                        _JobBuilder.AddField(parsedField.Key, parsedField.Value);
                    }

                    return _JobBuilder.GetJobCard();
                    
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
    }
}