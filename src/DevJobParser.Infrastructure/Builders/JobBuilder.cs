using System;
using DevJobParser.Core.DTO;
using DevJobParser.Infrastructure.Fields.Enums;
using DevJobParser.Infrastructure.HtmlExtractors;

namespace DevJobParser.Infrastructure.Builders;

public class JobCardBuilder
{
    private JobCard? _jobCard;

    public void AddField(JobFieldName name, string value)
    {
        if (_jobCard is null)
        {
            _jobCard = new JobCard();
        }

        switch (name)
        {
            case JobFieldName.Url:
                _jobCard.Url = value;
                break;
            case JobFieldName.Title:
                _jobCard.Url = value;
                break;
            case JobFieldName.Salary:
                _jobCard.Url = value;
                break;
            case JobFieldName.Company:
                _jobCard.Url = value;
                break;
            case JobFieldName.Description:
                _jobCard.Url = value;
                break;
        }
    }

    public void AddAdditionalField(Dictionary<string, string?> additionalFields)
    {
        _jobCard.AdditionalDetails = additionalFields;
    }

    public JobCard GetJobCard()
    {
        var returnedJobCard = _jobCard;
        _jobCard = null;

        return returnedJobCard;
    }
}
