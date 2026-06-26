using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DevJobParser.DTO;

namespace DevJobParser.Parsers
{


    public class WorkUaJobParser
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly HtmlParser _htmlParser = new();
        public string SearchLink { get; private set; }

        private readonly string _jobLinkSelector;

        private readonly Dictionary<string, string> _jobDetailSelectors;

        public WorkUaJobParser(string searchLink)
        {
            SearchLink = searchLink;
            _jobLinkSelector = "div#pjax-jobs-list > div.card h2 > a";
            _jobDetailSelectors = new Dictionary<string, string>()
            {
                { "jobTitle", "div.card h1" },
                { "jobCompanyName", "div.card ul li span.glyphicon-company + a span" },
                { "jobSalary", "div.card div.wordwrap > div.row + ul > li span[title=\"Зарплата\"] + span" },
                { "jobDescription", "div.card div.company-description" },
                { "jobPlaceOfWork", "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Місце роботи\"])" },
                { "jobTermsAndConditions", "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Умови й вимоги\"])" },
                { "jobLanguageKnowladge", "div.card div.wordwrap > div.row + ul > li:has(span[title=\"Знання мов\"])" },
                { "jobTagsOfSkillsCollection", "div.card div.wordwrap > ul + div > ul > li > span" },
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

            List<string>? jobLinkListOnCurrentPage;

            do
            {
                jobLinkListOnCurrentPage = GetJobLinkListOnPage(currentPageNumber);

                Console.WriteLine("Parsed page: " + currentPageNumber);

                currentPageNumber++;
                Thread.Sleep(1000);

            } while (jobLinkListOnCurrentPage != null);

            return parsedJobLinkList;
        }

        private List<string>? GetJobLinkListOnPage(int pageNumber)
        {
            var pageHtml = _httpClient.GetStringAsync(SearchLink + pageNumber).Result;

            var angleHtmlDocument = _htmlParser.ParseDocument(pageHtml);

            var jobLinkCollectionOnCurrentPage = angleHtmlDocument.QuerySelectorAll(_jobLinkSelector);

            if (jobLinkCollectionOnCurrentPage is null || jobLinkCollectionOnCurrentPage.Length == 0)
            {
                return null;
            }

            var parsedJobLinkList = new List<string>();

            foreach (var jobLink in jobLinkCollectionOnCurrentPage)
            {
                parsedJobLinkList.Add("https://www.work.ua" + jobLink.GetAttribute("href"));
            }

            return parsedJobLinkList;
        }

        private List<JobCard> GetJobDetailsList(List<string> parsedJobLinkList)
        {
            var parsedJobDetailsList = new List<JobCard>();

            foreach (var parsedJobLink in parsedJobLinkList)
            {
                var pageHtml = _httpClient.GetStringAsync(parsedJobLink).Result;

                var angleHtmlDocument = _htmlParser.ParseDocument(pageHtml);

                // Main fields
                var jobTitle = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobTitle"]);
                var jobCompanyName = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobCompanyName"]);
                var jobSalary = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobSalary"]);
                var jobDescription = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobDescription"]);

                // Additional fields
                var jobPlaceOfWork = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobPlaceOfWork"]);
                var jobTermsAndConditions = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobTermsAndConditions"]);
                var jobLanguageKnowladge = GetCleanTextFromSelector(angleHtmlDocument, _jobDetailSelectors["jobLanguageKnowladge"]);

                var jobTagsOfSkillsCollection = GetCleanTextListFromSelector(angleHtmlDocument, _jobDetailSelectors["jobTagsOfSkillsCollection"]);
                var jobTagsOfSkills = GetStringFromTextList(jobTagsOfSkillsCollection);

                var additionalDetails = new Dictionary<string, string>()
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

        private string GetCleanTextFromSelector(IHtmlDocument angleHtmlDocument, string selector)
        {
            var rawText = GetTextFromSelector(angleHtmlDocument, selector);

            if (rawText == null)
            {
                return "Інформація відсутня";
            }

            var cleanedText = CleanWhiteSpaces(rawText);
            
            return cleanedText;
        }

        private string? GetTextFromSelector(IHtmlDocument angleHtmlDocument, string selector)
        {
            return angleHtmlDocument?.QuerySelector(selector)?.Text();
        }

        private List<string> GetCleanTextListFromSelector(IHtmlDocument angleHtmlDocument, string selector)
        {
            var htmlElementCollection = angleHtmlDocument?.QuerySelectorAll(selector);

            var textList = new List<string>();

            foreach (var htmlElement in htmlElementCollection)
            {
                var cleanedText = CleanWhiteSpaces(htmlElement.Text());

                textList.Add(cleanedText);
            }

            return textList;
        }

        private string GetStringFromTextList(List<string> textCollection)
        {
            if (textCollection.Count == 0 || textCollection is null)
            {
                return "Інформація відсутня";
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

        private string CleanWhiteSpaces(string text)
        {
            return text.Trim();
        }
    }
}
