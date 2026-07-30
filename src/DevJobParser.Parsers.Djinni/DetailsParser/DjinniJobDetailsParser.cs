using Microsoft.Extensions.Logging;
using DevJobParser.Core.DTO;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;

namespace DevJobParser.Parsers.Djinni.DetailsParser;

public class DjinniJobDetailsParser
{
    private readonly ILogger<DjinniJobDetailsParser> _logger;
    private readonly Dictionary<string, ParsingRule> _jobDetailSelectors;
    private readonly SemaphoreSlim _throttler;
    private readonly IHttpContentLoader _httpContentLoader;
    private readonly HtmlParser _htmlParser;

    public DjinniJobDetailsParser(IHttpContentLoader httpContentLoader, ILogger<DjinniJobDetailsParser> logger, HtmlParser htmlParser)
    {
        _httpContentLoader = httpContentLoader;
        _logger = logger;
        _htmlParser = htmlParser;
        _throttler = new SemaphoreSlim(initialCount: 5);

        _jobDetailSelectors = new Dictionary<string, ParsingRule>()
        {
            {
                "jobTitle",
                new ParsingRule
                {
                    Selector = "div.job-post-page h1",
                    Strategy = new HtmlTextExtractor()
                }
            },
            {
                "jobCompanyName",
                new ParsingRule
                {
                    Selector = "div.job-post-page h1 + div > div > a",
                    Strategy = new HtmlTextExtractor()
                }
            },
            {
                "jobSalary",
                new ParsingRule
                {
                    Selector = "div.job-post-page > header div.col-auto span",
                    Strategy = new HtmlTextExtractor()
                }
            },
            {
                "jobDescription",
                new ParsingRule
                {
                    Selector = "div.page-content div.job-post__description",
                    Strategy = new HtmlTextExtractor()
                }
            },
            {
                "jobTermsAndConditions",
                new ParsingRule
                {
                    Selector = "div.job-post-page aside > div.card.card-body ul > li",
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
            string htmlPage = await _httpContentLoader.GetHtmlAsync(parsedJobLink, cancellationToken);

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
            List<string?>? jobTermsAndConditionsList = GetDataFromHtmlTags(angleHtmlDocument, _jobDetailSelectors["jobTermsAndConditions"]);
            string? jobTermsAndConditions = GetStringFromTextList(jobTermsAndConditionsList);

            var additionalDetails = new Dictionary<string, string?>()
            {
                { "termsAndConditions", jobTermsAndConditions },
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
