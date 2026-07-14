using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System.Reflection.Metadata;

namespace DevJobParser.Parsers
{
    internal class DjinniParser
    {
        public static List<Dictionary<string, string>> Start()
        {
            HttpClient httpClient = new HttpClient();
            HtmlParser parser = new();
            string searchLink = "https://djinni.co/jobs/?search_type=basic-search&primary_keyword=Blazor&primary_keyword=.NET%20Gamedev&primary_keyword=WinForms&primary_keyword=Dotnet%20Web&primary_keyword=Xamarin&primary_keyword=Dotnet%20Mobile&primary_keyword=Dotnet%20Desktop&primary_keyword=.NET&primary_keyword=WPF&primary_keyword=ASP.NET&primary_keyword=Dotnet%20Cloud&primary_keyword=MAUI&exp_level=no_exp&exp_level=1y&employment=remote&page=";

            int pageCount = 1;
            string jobLinkSelector = "div.job-item > div > a";
            List<string> parsedJobLinkList = new();

            var response1 = httpClient.GetStringAsync(searchLink + pageCount).Result;
            var document1 = parser.ParseDocument(response1);
            string foundJobs = document1.QuerySelector("header > div > h1 + span").Text();

            //Parse job cards
            
            while(true)
            {
                var response = httpClient.GetStringAsync(searchLink + pageCount).Result;

                var document = parser.ParseDocument(response);

                var jobLinkListOnPage = document.QuerySelectorAll(jobLinkSelector);

                string foundJobs1 = document.QuerySelector("header > div > h1+span").Text();

                if (foundJobs != foundJobs1)
                {
                    break;
                }

                foreach (var jobCard in jobLinkListOnPage)
                {
                    parsedJobLinkList.Add("https://djinni.co" + jobCard.GetAttribute("href"));
                }

                Console.WriteLine("Parsed page: " + pageCount);

                pageCount++;
                Thread.Sleep(1000);
            }

            foreach (var parsedLink in parsedJobLinkList)
            {
                Console.WriteLine(parsedLink);
            }

            var parsedJobList = new List<Dictionary<string, string>>();

            foreach (var parsedJobLink in parsedJobLinkList)
            {
                var parsedJob = new Dictionary<string, string>();
                var response = httpClient.GetStringAsync(parsedJobLink).Result;

                var document = parser.ParseDocument(response);

                var title = document?.QuerySelector("div.job-post-page h1")?.Text();
                var company = document?.QuerySelector("div.job-post-page h1 + div > div > a")?.Text();
                var salary = document?.QuerySelector("div.job-post-page > header div.col-auto span")?.Text() ?? "Невідомо";
                var termsAndConditionsCollection = document?.QuerySelectorAll("div.job-post-page aside > div.card.card-body ul > li");
                var description = document?.QuerySelector("div.page-content div.job-post__description")?.Text();

                string termsAndConditions = "";

                foreach (var tag in termsAndConditionsCollection)
                {
                    if (termsAndConditions != "")
                    {
                        termsAndConditions += ", ";
                    }
                    string[] words = tag.Text().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string trimmedTag = string.Join(" ", words);

                    termsAndConditions += trimmedTag;
                }

                parsedJob.Add("url", parsedJobLink);
                parsedJob.Add("title", title.Trim());
                parsedJob.Add("company", company.Trim());
                parsedJob.Add("salary", salary.Trim());
                parsedJob.Add("termsAndConditions", termsAndConditions);
                parsedJob.Add("description", description.Trim());

                parsedJobList.Add(parsedJob);
            }

            foreach (var parsedJob in parsedJobList)
            {
                var keys = parsedJob.Keys.ToArray();
                var values = parsedJob.Values.ToArray();

                for (var i = 0; parsedJob.Count > i; i++)
                {
                    Console.WriteLine($"{keys[i]}: {values[i]}");
                }

                Console.WriteLine("------------------------------------------------------------------------------");
            }

            return parsedJobList;
        }
    }
}
