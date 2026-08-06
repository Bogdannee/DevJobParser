using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace ConsoleApp2
{
    internal class Program
    {
        static async Task Main()
        {
            //await DjinniParser.Start();
            await DjinniParser.GetJob();
        }
    }
    internal class DjinniParser
    {
        public async static Task<List<Dictionary<string, string>>> Start()
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Cookie", "_hjSessionUser_2259799=eyJpZCI6ImNmODIyODVkLWE1NDgtNTg4Zi1hNDJkLWNkYzdkMzY3MzE2NiIsImNyZWF0ZWQiOjE3NTU0NTc2NDI3MzYsImV4aXN0aW5nIjp0cnVlfQ==; _ga=GA1.1.897511225.1776346063; _fbp=fb.1.1776346062956.330861965910088741; _gcl_au=1.1.1466098426.1784548509; _clck=1w1lczt%5E2%5Eg7w%5E0%5E2392; searchEventAction=no_suggest; cf_clearance=i_oiDSI2RwzlGvp95ObAKoKTIyFymgowsjoHczuja8g-1785241140-1.2.1.1-T0UTP1pzYHhZNqrrWUqXfdRJF4yKMxxzLvB3sMmPdtm.Rz8TtqM2.rTs1ywtm3FveEsO4HnbU3uvwiLohOT0Oaae9rS2Tqz6qz0sAkujOXm2XLOTv.KObpmBc6jhxBQe.25n0qJTOmjSlii5d.BQkidJvg2JZCxZswPy.0W0.vH4OgjJYcVnX4HRnBf2lWfPAsP3hFITwtnK5HMu7UJhpVcuxi2DcLqRFuBogIOgEmr40.kJPlnxUJf7hvZ6mN_FKKeUoUPw0lCi3Agy8p6SBQVn8O_bZUToKhulKHs0SmEnf8Nh1ZBhxF1z_MBOWPXMLWYZ5yWqyLit2CHZ9s8LDe5Ai3hmtZ2tfSgot51MnPAvmKFabW7N6sNpOcjgOQIxaFCNLvVpDdbMkubEVArAS.BRJBH7o2pZTBHD._WFi3fbRyOWGaqeP0GXrGInpE2CXbIdWsdK3fiuUxFwxCQoJw; __cf_bm=0wE9iaoRyQJ2BnOh2rX84llJ79A8nM95xmMwN7FgMnw-1785241140.1281357-1.0.1.1-9XV95ciICuy_EA9FRtK3FWIiB_JFAkPhZmv5RUuN_.S4VKosPVWSc1osiRSp8R_65r7yOevy23pCrJzH3Z6cI0YUCAmHInXsdTTaQYt4RbvSVzts7y.1cDTv5_7x.ttw; _ga_WS6TVT9PSM=GS2.1.s1785241118$o54$g1$t1785241140$j38$l0$h378327272$dvMayrGhoLVaKg2cTNjMWa5q-FDtGL7-rsQ");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");

            var graphQlQuery = """
            query getPublishedVacanciesList($filter: PublishedVacanciesFilterInput!, $pagination: PublishedVacanciesPaginationInput!, $sort: PublishedVacanciesSortType!, $isBrowser: Boolean!) {
                publishedVacancies(filter: $filter, pagination: $pagination, sort: $sort) {
                totalCount
                items {
                    ...PublishedVacanciesItem
                    __typename
                }
                __typename
                }
            }

            fragment PublishedVacanciesItem on Vacancy {
                id
                schedules {
                id
                __typename
                }
                title
                distanceText
                description
                showLogo
                sortDateText
                hot
                designBannerUrl
                isPublicationInAllCities
                badges {
                name
                __typename
                }
                salary {
                amount
                comment
                amountFrom
                amountTo
                __typename
                }
                company {
                id
                logoUrl
                name
                honors {
                    badge {
                    iconUrl
                    tooltipDescription
                    locations
                    isFavorite
                    __typename
                    }
                    __typename
                }
                __typename
                }
                city {
                id
                name
                __typename
                }
                showProfile
                seekerFavorite @include(if: $isBrowser) {
                isFavorite
                __typename
                }
                seekerDisliked @include(if: $isBrowser) {
                isDisliked
                __typename
                }
                formApplyCustomUrl
                anonymous
                isActive
                publicationType
                branding {
                ...PublishedVacancyBranding
                ...PublishedVacancyBrandingByStudio
                __typename
                }
                __typename
            }

            fragment PublishedVacancyBranding on VacancyBranding {
                id
                name
                banner {
                media {
                    ...PublishedVacancyBrandingMediaImage
                    __typename
                }
                __typename
                }
                __typename
            }

            fragment PublishedVacancyBrandingMediaImage on MediaImage {
                fileName
                url
                __typename
            }

            fragment PublishedVacancyBrandingByStudio on VacancyBrandingByStudio {
                id
                name
                bannerByStudio: banner {
                media {
                    ...PublishedVacancyBrandingMediaImage
                    __typename
                }
                __typename
                }
                __typename
            }
            """;

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
                            page = numPage
                        },
                        filter = new
                        {
                            keywords = ".net",
                            militaryVacancyDisplayMode = "EXCLUDED",
                            metroBranches = new object[0],         // Пустой массив в JSON
                            additionalKeywords = "",
                            clusterKeywords = new object[0],
                            salary = 0,
                            districtIds = new object[0],
                            microDistrictIds = new object[0],
                            scheduleIds = new[] { "3" },           // Массив со строкой
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
                            gender = (string)null,                 // ВАЖНО: для null в анонимных объектах нужен явный каст типов!
                            branchIds = new object[0]
                        },
                        sort = "BY_BUSINESS_SCORE",
                        isBrowser = true
                    },
                    query = graphQlQuery
                };



                // 3. Отправляем обычный POST запрос и сразу получаем JSON в виде строки (или десериализуем)
                var response = await httpClient.PostAsJsonAsync(
                    "https://dracula.robota.ua/?q=getPublishedVacanciesList",
                    payload);

                response.EnsureSuccessStatusCode();

                // Получаем чистый JSON-ответ в виде строки!
                string jsonResponse = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(jsonResponse);
                //await GetJob();

                JsonNode? rootNode = JsonNode.Parse(jsonResponse);

                var ids = new List<string>();
                var companyIds = new List<string>();

                var items = rootNode?["data"]?["publishedVacancies"]?["items"]?.AsArray();

                if (items.Count == 0)
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

                        if (vacancyId != null) ids.Add(vacancyId);
                        if (companyId != null) companyIds.Add(companyId);
                    }
                }

                foreach (var id in ids)
                {
                    await GetJob(id);
                    await Task.Delay(200);
                }

                Console.WriteLine($"\nPage: {numPage}\n");
            }
            return new List<Dictionary<string, string>>();
        }

        public static async Task<string> GetJob(string jobId)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Cookie", "_hjSessionUser_2259799=eyJpZCI6ImNmODIyODVkLWE1NDgtNTg4Zi1hNDJkLWNkYzdkMzY3MzE2NiIsImNyZWF0ZWQiOjE3NTU0NTc2NDI3MzYsImV4aXN0aW5nIjp0cnVlfQ==; _ga=GA1.1.897511225.1776346063; _fbp=fb.1.1776346062956.330861965910088741; _gcl_au=1.1.1466098426.1784548509; _clck=1w1lczt%5E2%5Eg7w%5E0%5E2392; searchEventAction=no_suggest; cf_clearance=i_oiDSI2RwzlGvp95ObAKoKTIyFymgowsjoHczuja8g-1785241140-1.2.1.1-T0UTP1pzYHhZNqrrWUqXfdRJF4yKMxxzLvB3sMmPdtm.Rz8TtqM2.rTs1ywtm3FveEsO4HnbU3uvwiLohOT0Oaae9rS2Tqz6qz0sAkujOXm2XLOTv.KObpmBc6jhxBQe.25n0qJTOmjSlii5d.BQkidJvg2JZCxZswPy.0W0.vH4OgjJYcVnX4HRnBf2lWfPAsP3hFITwtnK5HMu7UJhpVcuxi2DcLqRFuBogIOgEmr40.kJPlnxUJf7hvZ6mN_FKKeUoUPw0lCi3Agy8p6SBQVn8O_bZUToKhulKHs0SmEnf8Nh1ZBhxF1z_MBOWPXMLWYZ5yWqyLit2CHZ9s8LDe5Ai3hmtZ2tfSgot51MnPAvmKFabW7N6sNpOcjgOQIxaFCNLvVpDdbMkubEVArAS.BRJBH7o2pZTBHD._WFi3fbRyOWGaqeP0GXrGInpE2CXbIdWsdK3fiuUxFwxCQoJw; __cf_bm=0wE9iaoRyQJ2BnOh2rX84llJ79A8nM95xmMwN7FgMnw-1785241140.1281357-1.0.1.1-9XV95ciICuy_EA9FRtK3FWIiB_JFAkPhZmv5RUuN_.S4VKosPVWSc1osiRSp8R_65r7yOevy23pCrJzH3Z6cI0YUCAmHInXsdTTaQYt4RbvSVzts7y.1cDTv5_7x.ttw; _ga_WS6TVT9PSM=GS2.1.s1785241118$o54$g1$t1785241140$j38$l0$h378327272$dvMayrGhoLVaKg2cTNjMWa5q-FDtGL7-rsQ");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/150.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "uk-UA,uk;q=0.9,en-US;q=0.8,en;q=0.7");

            var graphQlQuery = """
                query getPublishedVacancy($id: ID!, $isBrowser: Boolean!, $trackView: Boolean) {
                  publishedVacancy(id: $id, trackView: $trackView) {
                    ...PublishedVacancyPage
                    __typename
                  }
                }

                fragment PublishedVacancyPage on Vacancy {
                  id
                  title
                  anonymous
                  showLogo
                  city {
                    id
                    name
                    __typename
                  }
                  company {
                    ...CompanyInfo
                    __typename
                  }
                  salary {
                    comment
                    amount
                    amountFrom
                    amountTo
                    __typename
                  }
                  isPublicationInAllCities
                  sortDateText
                  sortDate
                  address {
                    name
                    district {
                      name
                      __typename
                    }
                    metro {
                      name
                      __typename
                    }
                    longitude
                    latitude
                    __typename
                  }
                  distanceText
                  badges {
                    ...Badge
                    __typename
                  }
                  description
                  fullDescription
                  contacts {
                    name
                    phones
                    photo
                    socials
                    __typename
                  }
                  seekerDisliked @include(if: $isBrowser) {
                    isDisliked
                    __typename
                  }
                  seekerFavorite @include(if: $isBrowser) {
                    isFavorite
                    __typename
                  }
                  seekerApplication @include(if: $isBrowser) {
                    isApplied
                    lastTimeAppliedAt
                    __typename
                  }
                  isActive
                  hasDesign
                  designType
                  design {
                    ...HeaderInfo
                    id
                    backgroundHtml
                    footerInfo {
                      ...DesignFooterInfo
                      __typename
                    }
                    __typename
                  }
                  branch {
                    id
                    name
                    __typename
                  }
                  schedules {
                    id
                    name
                    __typename
                  }
                  hot
                  media {
                    ...MediaObject
                    __typename
                  }
                  ...KeyTagGroups
                  supportApplicationWithoutResume
                  formApplyCustomUrl
                  publicationType
                  candidatesScreening @include(if: $isBrowser) {
                    questionnaire {
                      id
                      __typename
                    }
                    isEnabled
                    __typename
                  }
                  status
                  branding {
                    ...VacancyBrandingItemByStudio
                    ...VacancyBrandingItem
                    __typename
                  }
                  __typename
                }

                fragment CompanyInfo on Company {
                  id
                  logoUrl
                  name
                  isVerified
                  companyUrl
                  miniProfile {
                    ...CompanyMiniProfileInfo
                    __typename
                  }
                  honors {
                    ...CompanyHonors
                    __typename
                  }
                  __typename
                }

                fragment CompanyMiniProfileInfo on CompanyMiniProfile {
                  isEnabled
                  description
                  images
                  years
                  benefits {
                    name
                    id
                    __typename
                  }
                  staffSize {
                    id
                    name
                    __typename
                  }
                  __typename
                }

                fragment CompanyHonors on CompanyHonors {
                  badge {
                    iconUrl
                    locations
                    isFavorite
                    tooltipDescription
                    __typename
                  }
                  __typename
                }

                fragment Badge on PublishedVacancyBadge {
                  name
                  id
                  __typename
                }

                fragment HeaderInfo on VacancyDesign {
                  headerInfo {
                    ...DesignHeaderInfo
                    __typename
                  }
                  __typename
                }

                fragment DesignHeaderInfo on VacancyDesignHeader {
                  mediaItems {
                    type
                    url
                    videoCoverImageUrl
                    __typename
                  }
                  videoPlayButtonImageUrl
                  __typename
                }

                fragment DesignFooterInfo on VacancyDesignFooter {
                  imageUrl
                  __typename
                }

                fragment MediaObject on VacancyMedia {
                  url
                  description
                  type
                  __typename
                }

                fragment KeyTagGroups on Vacancy {
                  keyTagGroups {
                    name
                    id
                    __typename
                  }
                  __typename
                }

                fragment VacancyBrandingItemByStudio on VacancyBrandingByStudio {
                  id
                  name
                  background {
                    ...VacancyBrandingBackgroundColorItem
                    ...VacancyBrandingBackgroundImageItem
                    ...VacancyBrandingBackgroundHtmlItem
                    __typename
                  }
                  footer {
                    media {
                      ...VacancyBrandingMediaImage
                      __typename
                    }
                    __typename
                  }
                  headerByStudio: header {
                    media {
                      ...VacancyBrandingMediaImage
                      ...VacancyBrandingMediaVideo
                      __typename
                    }
                    __typename
                  }
                  __typename
                }

                fragment VacancyBrandingBackgroundColorItem on VacancyBrandingBackgroundColor {
                  color {
                    color
                    __typename
                  }
                  __typename
                }

                fragment VacancyBrandingBackgroundImageItem on VacancyBrandingBackgroundImage {
                  fillingType
                  image {
                    fileName
                    url
                    __typename
                  }
                  __typename
                }

                fragment VacancyBrandingBackgroundHtmlItem on VacancyBrandingBackgroundHtml {
                  html
                  __typename
                }

                fragment VacancyBrandingMediaImage on MediaImage {
                  fileName
                  url
                  __typename
                }

                fragment VacancyBrandingMediaVideo on MediaVideo {
                  videoUrl: url
                  cover {
                    url
                    fileName
                    __typename
                  }
                  __typename
                }

                fragment VacancyBrandingItem on VacancyBranding {
                  id
                  name
                  background {
                    ...VacancyBrandingBackgroundColorItem
                    ...VacancyBrandingBackgroundImageItem
                    __typename
                  }
                  footer {
                    media {
                      ...VacancyBrandingMediaImage
                      __typename
                    }
                    __typename
                  }
                  header {
                    media {
                      ...VacancyBrandingMediaImage
                      ...VacancyBrandingMediaVideo
                      __typename
                    }
                    __typename
                  }
                  __typename
                }
                """;

            // 2. Собираем сам анонимный объект
            var vacancyPayload = new
            {
                operationName = "getPublishedVacancy",
                variables = new
                {
                    id = jobId,
                    trackView = false,
                    isBrowser = true
                },
                query = graphQlQuery
            };

            var response = await httpClient.PostAsJsonAsync(
                "https://dracula.robota.ua/?q=getPublishedVacancy",
                vacancyPayload);

            response.EnsureSuccessStatusCode();

            // Получаем чистый JSON-ответ в виде строки!
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
                    decimal? salaryAmount = vacancy["salary"]?["amount"]?.GetValue<decimal>();

                    // Вытаскиваем данные из вложенных объектов (например, город или зарплата)
                    string? description = vacancy["description"]?.ToString();

                    var tagGroupArray = vacancy?["keyTagGroups"]?.AsArray();
                    List<string> tagNames = tagGroupArray
                        ?.Select(tag => tag?["name"]?.ToString())
                        ?.Where(name => name != null)
                        ?.ToList() ?? new List<string>();

                    string allTagsString = string.Join(", ", tagNames);

                    // Выводим результат в консоль
                    Console.WriteLine($"ID Вакансии: {id}");
                    Console.WriteLine($"Название: {title}");
                    Console.WriteLine($"Название: {company}");
                    Console.WriteLine($"Город: {description}");
                    Console.WriteLine($"Зарплата: {salaryAmount} грн");
                    Console.WriteLine($"Доп: {allTagsString}");

                    // Если нужно достать элемент из массива (например, первый график работы)
                    var firstSchedule = vacancy["schedules"]?[0]?["name"]?.ToString();
                    Console.WriteLine($"График: {firstSchedule}");
                }
            }
            Console.WriteLine("-------------------------");
            return jsonResponse;
        }
    }
}