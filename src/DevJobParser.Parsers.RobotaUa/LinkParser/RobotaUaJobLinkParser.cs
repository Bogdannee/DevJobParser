using System;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DevJobParser.Infrastructure.Fields;
using DevJobParser.Infrastructure.Fields.Enums;
using DevJobParser.Infrastructure.HtmlExtractors;
using DevJobParser.Infrastructure.Loading;
using Microsoft.Extensions.Logging;

namespace DevJobParser.Parsers.RobotaUa.LinkParser;

public class RobotaUaJobLinkParser
{
    private readonly ILogger<RobotaUaJobLinkParser> _logger;
    private readonly IHttpContentLoader _httpContentLoader;

    public RobotaUaJobLinkParser(
        ILogger<RobotaUaJobLinkParser> logger,
        IHttpContentLoader httpContentLoader,
        HtmlExtractController htmlExtractController)
    {
        _logger = logger;
        _httpContentLoader = httpContentLoader;
    }

    public async Task<List<RobotaUaJobLinkResponse>> GetJobLinksFromSearchLink(string searchLink, int maxPages, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Cookie", "_hjSessionUser_2259799=eyJpZCI6ImNmODIyODVkLWE1NDgtNTg4Zi1hNDJkLWNkYzdkMzY3MzE2NiIsImNyZWF0ZWQiOjE3NTU0NTc2NDI3MzYsImV4aXN0aW5nIjp0cnVlfQ==; _ga=GA1.1.897511225.1776346063; _fbp=fb.1.1776346062956.330861965910088741; _gcl_au=1.1.1466098426.1784548509; _clck=1w1lczt%5E2%5Eg7w%5E0%5E2392; searchEventAction=no_suggest; cf_clearance=i_oiDSI2RwzlGvp95ObAKoKTIyFymgowsjoHczuja8g-1785241140-1.2.1.1-T0UTP1pzYHhZNqrrWUqXfdRJF4yKMxxzLvB3sMmPdtm.Rz8TtqM2.rTs1ywtm3FveEsO4HnbU3uvwiLohOT0Oaae9rS2Tqz6qz0sAkujOXm2XLOTv.KObpmBc6jhxBQe.25n0qJTOmjSlii5d.BQkidJvg2JZCxZswPy.0W0.vH4OgjJYcVnX4HRnBf2lWfPAsP3hFITwtnK5HMu7UJhpVcuxi2DcLqRFuBogIOgEmr40.kJPlnxUJf7hvZ6mN_FKKeUoUPw0lCi3Agy8p6SBQVn8O_bZUToKhulKHs0SmEnf8Nh1ZBhxF1z_MBOWPXMLWYZ5yWqyLit2CHZ9s8LDe5Ai3hmtZ2tfSgot51MnPAvmKFabW7N6sNpOcjgOQIxaFCNLvVpDdbMkubEVArAS.BRJBH7o2pZTBHD._WFi3fbRyOWGaqeP0GXrGInpE2CXbIdWsdK3fiuUxFwxCQoJw; __cf_bm=0wE9iaoRyQJ2BnOh2rX84llJ79A8nM95xmMwN7FgMnw-1785241140.1281357-1.0.1.1-9XV95ciICuy_EA9FRtK3FWIiB_JFAkPhZmv5RUuN_.S4VKosPVWSc1osiRSp8R_65r7yOevy23pCrJzH3Z6cI0YUCAmHInXsdTTaQYt4RbvSVzts7y.1cDTv5_7x.ttw; _ga_WS6TVT9PSM=GS2.1.s1785241118$o54$g1$t1785241140$j38$l0$h378327272$dvMayrGhoLVaKg2cTNjMWa5q-FDtGL7-rsQ");
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");

        for (int numPage = 0; numPage < 10; numPage++)
        {
            var payload = new
            {
                operationName = "getPublishedVacanciesList",
                variables = new
                {
                    pagination = new
                    {
                        count = 20,
                        page = numPage      // Текущая страница
                    },
                    filter = new
                    {
                        keywords = ".net",
                        militaryVacancyDisplayMode = "EXCLUDED",
                        metroBranches = new object[0],
                        additionalKeywords = "",
                        clusterKeywords = new object[0],
                        salary = 0,
                        districtIds = new object[0],
                        microDistrictIds = new object[0],
                        scheduleIds = new[] { "3" },
                        rubrics = new object[0],
                        showAgencies = true,
                        showOnlyNoCvApplyVacancies = false,
                        showOnlySpecialNeeds = false,
                        showOnlyWithoutExperience = false,
                        showOnlyNotViewed = false,
                        showWithoutSalary = true,
                        location = new
                        {
                            latitude = 0,
                            longitude = 0
                        },
                        isForVeterans = false,
                        isReservation = false,
                        isOfficeWithGenerator = false,
                        isOfficeWithShelter = false,
                        gender = (string)null,
                        branchIds = new object[0]
                    },
                    sort = "BY_BUSINESS_SCORE",
                    isBrowser = true
                },
                query = RobotaUaJobLinkQueryOptions.graphQlQuery
            };

            // 3. Отправляем POST запрос и получаем JSON в виде строки (или десериализуем)
            var response = await httpClient.PostAsJsonAsync(
                "https://dracula.robota.ua/?q=getPublishedVacanciesList",
                payload);

            response.EnsureSuccessStatusCode();

            // Получаем чистый JSON-ответ в виде строки
            string jsonResponse = await response.Content.ReadAsStringAsync();

            JsonNode? rootNode = JsonNode.Parse(jsonResponse);

            var allIdsList = new List<RobotaUaJobLinkResponse>();
            var items = rootNode?["data"]?["publishedVacancies"]?["items"]?.AsArray();

            if (items == null || items.Count == 0)
            {
                break;
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item == null) continue;

                    var vacancyId = item["id"]?.ToString();
                    var companyId = item["company"]?["id"]?.ToString();

                    string url = $"https://robota.ua/company{companyId}/vacancy{vacancyId}";

                    allIdsList.Add(new RobotaUaJobLinkResponse(){
                        JobUrl = url,
                        JobId = vacancyId,
                    });
                }
            }

            Console.WriteLine($"\nPage: {numPage}\n");
        }
        return new List<RobotaUaJobLinkResponse>();
    }
}