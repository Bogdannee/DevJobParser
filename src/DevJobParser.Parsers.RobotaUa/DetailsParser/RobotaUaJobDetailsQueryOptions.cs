namespace DevJobParser.Parsers.RobotaUa.LinkParser;

public static class RobotaUaJobDetailsQueryOptions
{
    public static string graphQlQuery {get;} = """
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
}