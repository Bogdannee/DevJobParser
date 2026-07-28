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
            await DjinniParser.Start();
        }
    }
    internal class DjinniParser
    {
        public async static Task<List<Dictionary<string, string>>> Start()
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Cookie", "_hjSessionUser_2259799=eyJpZCI6ImNmODIyODVkLWE1NDgtNTg4Zi1hNDJkLWNkYzdkMzY3MzE2NiIsImNyZWF0ZWQiOjE3NTU0NTc2NDI3MzYsImV4aXN0aW5nIjp0cnVlfQ==; _ga=GA1.1.897511225.1776346063; _fbp=fb.1.1776346062956.330861965910088741; _gcl_au=1.1.1466098426.1784548509; searchEventAction=no_suggest; _clck=1w1lczt%5E2%5Eg7w%5E0%5E2392; _clsk=18lv4ex%5E1784548904379%5E1%5E1%5Et.clarity.ms%2Fcollect; cf_clearance=NhQZ02JZNQ6cHV.bPmO0xu2hWsoqQ7gj4RvZO5lbN6A-1784548914-1.2.1.1-LXWfxGxur7ABhUygbAbD1Uc0GuAsC8VLMBeRMop8R1oJitOZpnEHZCZ33zykSGyYyDSkhbtBUl5CHnzZrtseBhPSHwAhoqUntVAx6n0NSqUWQw6jQID7h.G2FSYNP3wFdYxHzAsyoPPkAjFzUsqelLhODM4ncOCpDjuk9nPT4EY4x.Hwq3xf9HYmZEwLDa1AXuH2fZ_La0xIeFleF41V7wCFbYGhxuXVkdVS0CK.d5Nbhhd_BKZqjFfTjuT5ju0Zvz.iNPVwje45md48IKrMlWJ6Y1P4yGDgls7rqbtkuEnpXeQEDolCdVkCyoyXjKdLfmWSLxi3SoSZpfShpo5TCHOgd1zW3PhHcrF4veAymWxSZZTGxM0HhbtCJ_eOpQ.EpPsC2sLS3LMyxrvG1jrjXpx7PhXenaTayceOHbl0PqXgGEe_n3IWq6Z.fMbD42L6; __cf_bm=1QFnz5u9SrzqICAuDoQCvBu_ob91rRcSPsT.VRsQX1I-1784548919.3992708-1.0.1.1-WrD7Dh8Mlvs7SM6MSxUOEM7FFKF5cF.q6WqTRdabDNfx.M_rGe1.QXV8YaylQ_lAnDQdphO4feroosm.DLPIotXYfARUtWbJ90LZePpSMUJ1.NVeS4IwsPsyop0PYsWB; _ga_WS6TVT9PSM=GS2.1.s1784548509$o50$g1$t1784548919$j34$l0$h465888408$dvMayrGhoLVaKg2cTNjMWa5q-FDtGL7-rsQ");
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

            var payload = new
            {
                operationName = "getPublishedVacanciesList",
                variables = new
                {
                    pagination = new
                    {
                        count = 20,
                        page = 1
                    },
                    filter = new
                    {
                        keywords = ".net",
                        militaryVacancyDisplayMode = "APPENDED",
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
            //var response = await httpClient.PostAsJsonAsync(
            //    "https://dracula.robota.ua/?q=getPublishedVacanciesList",
            //    payload);

            //response.EnsureSuccessStatusCode();

            //// Получаем чистый JSON-ответ в виде строки!
            //string jsonResponse = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(jsonResponse);
            await GetJob();
            return new List<Dictionary<string, string>>();
        }

        static async Task<string> GetJob()
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Cookie", "_hjSessionUser_2259799=eyJpZCI6ImNmODIyODVkLWE1NDgtNTg4Zi1hNDJkLWNkYzdkMzY3MzE2NiIsImNyZWF0ZWQiOjE3NTU0NTc2NDI3MzYsImV4aXN0aW5nIjp0cnVlfQ==; _ga=GA1.1.897511225.1776346063; _fbp=fb.1.1776346062956.330861965910088741; _gcl_au=1.1.1466098426.1784548509; searchEventAction=no_suggest; _clck=1w1lczt%5E2%5Eg7w%5E0%5E2392; _clsk=18lv4ex%5E1784548904379%5E1%5E1%5Et.clarity.ms%2Fcollect; cf_clearance=NhQZ02JZNQ6cHV.bPmO0xu2hWsoqQ7gj4RvZO5lbN6A-1784548914-1.2.1.1-LXWfxGxur7ABhUygbAbD1Uc0GuAsC8VLMBeRMop8R1oJitOZpnEHZCZ33zykSGyYyDSkhbtBUl5CHnzZrtseBhPSHwAhoqUntVAx6n0NSqUWQw6jQID7h.G2FSYNP3wFdYxHzAsyoPPkAjFzUsqelLhODM4ncOCpDjuk9nPT4EY4x.Hwq3xf9HYmZEwLDa1AXuH2fZ_La0xIeFleF41V7wCFbYGhxuXVkdVS0CK.d5Nbhhd_BKZqjFfTjuT5ju0Zvz.iNPVwje45md48IKrMlWJ6Y1P4yGDgls7rqbtkuEnpXeQEDolCdVkCyoyXjKdLfmWSLxi3SoSZpfShpo5TCHOgd1zW3PhHcrF4veAymWxSZZTGxM0HhbtCJ_eOpQ.EpPsC2sLS3LMyxrvG1jrjXpx7PhXenaTayceOHbl0PqXgGEe_n3IWq6Z.fMbD42L6; __cf_bm=1QFnz5u9SrzqICAuDoQCvBu_ob91rRcSPsT.VRsQX1I-1784548919.3992708-1.0.1.1-WrD7Dh8Mlvs7SM6MSxUOEM7FFKF5cF.q6WqTRdabDNfx.M_rGe1.QXV8YaylQ_lAnDQdphO4feroosm.DLPIotXYfARUtWbJ90LZePpSMUJ1.NVeS4IwsPsyop0PYsWB; _ga_WS6TVT9PSM=GS2.1.s1784548509$o50$g1$t1784548919$j34$l0$h465888408$dvMayrGhoLVaKg2cTNjMWa5q-FDtGL7-rsQ");
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
                    id = "11112632",
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
                    bool? isActive = vacancy["isActive"]?.GetValue<bool>();

                    // Вытаскиваем данные из вложенных объектов (например, город или зарплата)
                    string? cityName = vacancy["city"]?["name"]?.ToString();
                    decimal? salaryAmount = vacancy["salary"]?["amount"]?.GetValue<decimal>();

                    // Выводим результат в консоль
                    Console.WriteLine($"ID Вакансии: {id}");
                    Console.WriteLine($"Название: {title}");
                    Console.WriteLine($"Город: {cityName}");
                    Console.WriteLine($"Зарплата: {salaryAmount} грн");
                    Console.WriteLine($"Активна: {isActive}");

                    // Если нужно достать элемент из массива (например, первый график работы)
                    var firstSchedule = vacancy["schedules"]?[0]?["name"]?.ToString();
                    Console.WriteLine($"График: {firstSchedule}");
                }
            }

            return jsonResponse;
        }
    }
}