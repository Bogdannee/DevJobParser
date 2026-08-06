namespace DevJobParser.Parsers.RobotaUa.LinkParser;

public static class RobotaUaJobLinkQueryOptions
{
    public static string graphQlQuery {get;} = """
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
}