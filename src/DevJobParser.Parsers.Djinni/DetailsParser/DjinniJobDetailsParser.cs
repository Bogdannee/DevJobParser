using Microsoft.Extensions.Logging;
using DevJobParser.Core.DTO;
using DevJobParser.Core.Exceptions;
using DevJobParser.Infrastructure.Loading;
using DevJobParser.Infrastructure.HtmlExtractors;
using AngleSharp.Html.Parser;
using AngleSharp.Html.Dom;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;
using DevJobParser.Infrastructure.Builders;

namespace DevJobParser.Parsers.Djinni.DetailsParser;

public class DjinniJobDetailsParser
{
    private readonly ILogger<DjinniJobDetailsParser> _logger;
    private readonly List<AbstractHtmlField> _jobDetailSelectors;
    private readonly SemaphoreSlim _throttler;
    private readonly IHttpContentLoader _httpContentLoader;
    private readonly HtmlExtractController _htmlExtractController;
    private readonly JobCardBuilder _jobCardBuilder;

    public DjinniJobDetailsParser(
        IHttpContentLoader httpContentLoader,
        ILogger<DjinniJobDetailsParser> logger,
        HtmlExtractController htmlExtractController,
        JobCardBuilder jobCardBuilder)
    {
        _httpContentLoader = httpContentLoader;
        _logger = logger;
        _htmlExtractController = htmlExtractController;
        _jobCardBuilder = jobCardBuilder;
        _throttler = new SemaphoreSlim(initialCount: 5);

        _jobDetailSelectors = new List<AbstractHtmlField>()
        {
            new HtmlField(JobFieldName.Title)
            {
                Selector = "div.job-post-page h1",
                Strategy = new HtmlTextExtractor(),
                Quantity = ValueQuantity.Single
            },
            new HtmlField(JobFieldName.Company)
            {
                Selector = "div.job-post-page h1 + div > div > a",
                Strategy = new HtmlTextExtractor(),
                Quantity = ValueQuantity.Single
            },
            new HtmlField(JobFieldName.Salary)
            {
                Selector = "div.job-post-page > header div.col-auto span",
                Strategy = new HtmlTextExtractor(),
                Quantity = ValueQuantity.Single
            },
            new HtmlField(JobFieldName.Description)
            {
                Selector = "div.page-content div.job-post__description",
                Strategy = new HtmlTextExtractor(),
                Quantity = ValueQuantity.Single
            },
            new HtmlAdditionalField()
            {
                AdditionalDetailName = "Terms And Conditions",
                Selector = "div.job-post-page aside > div.card.card-body ul > li",
                Strategy = new HtmlTextExtractor(),
                Quantity = ValueQuantity.Multiply
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
                var parsedMainFields = new Dictionary<JobFieldName, string?>();  
                var additionalDetails = new Dictionary<string, string?>();

                string htmlPage = await _httpContentLoader.GetHtmlAsync(parsedJobLink, cancellationToken);

                _htmlExtractController.ParseDocument(htmlPage);

                // Main fields
                parsedMainFields.Add(JobFieldName.Url, parsedJobLink);
                
                foreach (var field in _jobDetailSelectors)
                {
                    var htmlField = field as HtmlField;

                    if (htmlField != null)
                    {
                        string? parsedField = _htmlExtractController.GetStringifiedField(htmlField);
                        parsedMainFields.Add(htmlField.Name, parsedField);
                    }
                }
                
                // Additional fields
                foreach (var field in _jobDetailSelectors)
                {
                    var additionalField = field as HtmlAdditionalField;

                    if (additionalField != null)
                    {
                        string? parsedField = _htmlExtractController.GetStringifiedField(additionalField);
                        additionalDetails.Add(additionalField.AdditionalDetailName, parsedField);
                    }
                }

                foreach (var parsedField in parsedMainFields)
                {
                    _jobCardBuilder.AddField(parsedField.Key, parsedField.Value);
                }

                _jobCardBuilder.AddAdditionalField(additionalDetails);

                return _jobCardBuilder.GetJobCard();    

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
