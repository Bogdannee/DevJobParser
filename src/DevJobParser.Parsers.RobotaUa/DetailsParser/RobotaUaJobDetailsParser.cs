using System;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using DevJobParser.Core.DTO;
using DevJobParser.Infrastructure.Builders;
using DevJobParser.Parsers.RobotaUa.LinkParser;

namespace DevJobParser.Parsers.RobotaUa.DetailsParser;

public class RobotaUaJobDetailsParser
{
    private readonly JobCardBuilder _jobCardBuilder;

    public RobotaUaJobDetailsParser(JobCardBuilder jobCardBuilder)
    {
        _jobCardBuilder = jobCardBuilder;
    }

    public async Task<List<JobCard>> GetJobDetailsList(List<RobotaUaJobLinkResponse> parsedJobLinkList, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Cookie", "_hjSessionUser_2259799=eyJpZCI6ImNmODIyODVkLWE1NDgtNTg4Zi1hNDJkLWNkYzdkMzY3MzE2NiIsImNyZWF0ZWQiOjE3NTU0NTc2NDI3MzYsImV4aXN0aW5nIjp0cnVlfQ==; _ga=GA1.1.897511225.1776346063; _fbp=fb.1.1776346062956.330861965910088741; _gcl_au=1.1.1466098426.1784548509; _clck=1w1lczt%5E2%5Eg7w%5E0%5E2392; searchEventAction=no_suggest; cf_clearance=i_oiDSI2RwzlGvp95ObAKoKTIyFymgowsjoHczuja8g-1785241140-1.2.1.1-T0UTP1pzYHhZNqrrWUqXfdRJF4yKMxxzLvB3sMmPdtm.Rz8TtqM2.rTs1ywtm3FveEsO4HnbU3uvwiLohOT0Oaae9rS2Tqz6qz0sAkujOXm2XLOTv.KObpmBc6jhxBQe.25n0qJTOmjSlii5d.BQkidJvg2JZCxZswPy.0W0.vH4OgjJYcVnX4HRnBf2lWfPAsP3hFITwtnK5HMu7UJhpVcuxi2DcLqRFuBogIOgEmr40.kJPlnxUJf7hvZ6mN_FKKeUoUPw0lCi3Agy8p6SBQVn8O_bZUToKhulKHs0SmEnf8Nh1ZBhxF1z_MBOWPXMLWYZ5yWqyLit2CHZ9s8LDe5Ai3hmtZ2tfSgot51MnPAvmKFabW7N6sNpOcjgOQIxaFCNLvVpDdbMkubEVArAS.BRJBH7o2pZTBHD._WFi3fbRyOWGaqeP0GXrGInpE2CXbIdWsdK3fiuUxFwxCQoJw; __cf_bm=0wE9iaoRyQJ2BnOh2rX84llJ79A8nM95xmMwN7FgMnw-1785241140.1281357-1.0.1.1-9XV95ciICuy_EA9FRtK3FWIiB_JFAkPhZmv5RUuN_.S4VKosPVWSc1osiRSp8R_65r7yOevy23pCrJzH3Z6cI0YUCAmHInXsdTTaQYt4RbvSVzts7y.1cDTv5_7x.ttw; _ga_WS6TVT9PSM=GS2.1.s1785241118$o54$g1$t1785241140$j38$l0$h378327272$dvMayrGhoLVaKg2cTNjMWa5q-FDtGL7-rsQ");
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36");
        httpClient.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");

        var jobCardList = new List<JobCard>();

        foreach (var item in parsedJobLinkList)
        {
            // Payload
            var vacancyPayload = new
            {
                operationName = "getPublishedVacancy",
                variables = new
                {
                    id = item.JobId,
                    trackView = false,
                    isBrowser = true
                },
                query = RobotaUaJobDetailsQueryOptions.graphQlQuery
            };
            var response = await httpClient.PostAsJsonAsync(
            "https://dracula.robota.ua/?q=getPublishedVacancy",
            vacancyPayload);

            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

            JsonNode? rootNode = JsonNode.Parse(jsonResponse);

            if (rootNode != null)
            {
                // Шагаем по дереву JSON вглубь: data -> publishedVacancy
                var vacancy = rootNode["data"]?["publishedVacancy"];

                if (vacancy != null)
                {
                    // Вытаскиваем простые текстовые и числовые поля
                    string? id = vacancy["id"]?.ToString();
                    string? title = vacancy["title"]?.ToString();
                    string? company = vacancy["company"]?["name"]?.ToString();
                    string? salaryAmount = vacancy["salary"]?["amount"]?.ToString();

                    // Вытаскиваем данные из вложенных объектов (например, город или зарплата)
                    string? description = vacancy["description"]?.ToString();

                    var tagGroupArray = vacancy?["keyTagGroups"]?.AsArray();
                    List<string> tagNames = tagGroupArray
                        ?.Select(tag => tag?["name"]?.ToString())
                        ?.Where(name => name != null)
                        ?.ToList() ?? new List<string>();

                    string allTagsString = string.Join(", ", tagNames);
                    var jobCard = new JobCard()
                    {
                        Url = item.JobUrl,
                        Title = title,
                        Company = company,
                        Salary = salaryAmount,
                        AdditionalDetails = new Dictionary<string, string?>(){["Skills"] = allTagsString},
                    };
                    jobCardList.Add(jobCard);
                    // Если нужно достать элемент из массива (например, первый график работы)
                    var firstSchedule = vacancy["schedules"]?[0]?["name"]?.ToString();
                    Console.WriteLine($"График: {firstSchedule}");
                }
            }
        }
        
        return jobCardList;
    }
}
