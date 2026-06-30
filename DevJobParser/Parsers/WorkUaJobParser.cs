using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DevJobParser.DTO;
using DevJobParser.Parsers;

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

            return cleanedText;
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

        public HtmlTagGetAttribute(string attributeName)
        {
            _attributeName = attributeName;
        }

        public string? RetrieveData(IElement element)
        {
            return element?.GetAttribute(_attributeName);
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

    public class WorkUaJobParser
    {
        private readonly HttpClient _httpClient = new();
        private readonly HtmlParser _htmlParser = new();
        public string SearchLink { get; private set; }

        private readonly Dictionary<string, ParsingRule> _jobLinkSelector;

        private readonly Dictionary<string, ParsingRule> _jobDetailSelectors;

        public WorkUaJobParser(string searchLink)
        {
            SearchLink = searchLink;
            _jobLinkSelector = new Dictionary<string, ParsingRule>()
            {
                {
                    "JobLink",
                    new ParsingRule
                    {
                        Selector = "div#pjax-jobs-list > div.card h2 > a",
                        Strategy = new HtmlTagGetAttribute("href")
                    }
                }
            };

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

        public List<JobCard> GetJobCardList()
        {
            List<string> parsedJobLinkList = GetJobLinksFromSearchLink();
            List<JobCard> parsedJobDetailsList = GetJobDetailsList(parsedJobLinkList);

            return parsedJobDetailsList;
        }

        private List<string> GetJobLinksFromSearchLink()
        {
            int currentPageNumber = 1;
            var parsedJobLinkList = new List<string>();

            while(true)
            {
                List<string>? jobLinkListOnCurrentPage = GetJobLinkListOnPage(currentPageNumber);

                Console.WriteLine("Parsed page: " + currentPageNumber);

                currentPageNumber++;
                Thread.Sleep(1000);

                if (jobLinkListOnCurrentPage is null)
                {
                    break;
                }

                parsedJobLinkList.AddRange(jobLinkListOnCurrentPage);
            }

            return parsedJobLinkList;
        }

        private List<string?>? GetJobLinkListOnPage(int pageNumber)
        {
            var htmlPage = _httpClient.GetStringAsync(SearchLink + pageNumber).Result;
            var htmlDocumentObject = _htmlParser.ParseDocument(htmlPage);

            if (!_jobLinkSelector.TryGetValue("JobLink", out var linkRule))
            {
                throw new Exception("Правило для парсинга \'JobLink\' не настроено в словаре!");
            }

            var htmlJobLinksOnPage = htmlDocumentObject.QuerySelectorAll(linkRule.Selector);

            if (htmlJobLinksOnPage.Length == 0)
            {
                return null;
            }

            List<string?>? rawLinks = linkRule.Strategy.RetrieveData(htmlJobLinksOnPage);

            return rawLinks.Select(link => "https://www.work.ua" + link).ToList();
        }

        private List<JobCard> GetJobDetailsList(List<string> parsedJobLinkList)
        {
            var parsedJobDetailsList = new List<JobCard>();

            foreach (var parsedJobLink in parsedJobLinkList)
            {
                var pageHtml = _httpClient.GetStringAsync(parsedJobLink).Result;

                var angleHtmlDocument = _htmlParser.ParseDocument(pageHtml);

                // Main fields
                string jobTitle = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobTitle"]);
                string jobCompanyName = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobCompanyName"]);
                string? jobSalary = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobSalary"]);
                string jobDescription = GetDataFromHtmlTag(angleHtmlDocument, _jobDetailSelectors["jobDescription"]);

                if (jobTitle is null || jobCompanyName is null || jobDescription is null)
                {
                    throw new Exception("Main fields not parsed");
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

                var workUaJobCard = new JobCard()
                {
                    Url = parsedJobLink,
                    Title = jobTitle,
                    Company = jobCompanyName,
                    Salary = jobSalary,
                    Description = jobDescription,
                    AdditionalDetails = additionalDetails,
                };

                parsedJobDetailsList.Add(workUaJobCard);
            }

            return parsedJobDetailsList;
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

            string joinedTextCollection = "";

            foreach (var text in textCollection)
            {
                if (joinedTextCollection != "")
                {
                    joinedTextCollection += ", ";
                }

                joinedTextCollection += text;
            }

            return joinedTextCollection;
        }
    }
}