using System;
using AngleSharp.Browser;
using DevJobParser.Core.Abstractions;
using DevJobParser.Core.DTO;
using DevJobParser.Parsers.RobotaUa.DetailsParser;
using DevJobParser.Parsers.RobotaUa.LinkParser;
using DevJobParser.Parsers.RobotaUa.Options;
using Microsoft.Extensions.Options;

namespace DevJobParser.Parsers.RobotaUa;

public class RobotaUaJobParser : IJobParser
{
    private readonly RobotaUaJobLinkParser _robotaUaJobLinkParser;
    private readonly RobotaUaJobDetailsParser _robotaUaJobDetailsParser;
    private readonly string _searchlink;
    private readonly int _maxPages;
    public string SourceName {get; private set;}

    public RobotaUaJobParser(
        RobotaUaJobLinkParser robotaUaJobLinkParser,
        RobotaUaJobDetailsParser robotaUaJobDetailsParser,
        IOptions<RobotaUaParserOptions> options)
    {
        _robotaUaJobLinkParser = robotaUaJobLinkParser;
        _robotaUaJobDetailsParser = robotaUaJobDetailsParser;
        _searchlink = options.Value.SearchLink;
        _maxPages = options.Value.MaxPages;
        SourceName = options.Value.SourceName;
    }

    public async Task<List<JobCard>> GetJobCardList(CancellationToken cancellationToken)
    {
        List<RobotaUaJobLinkResponse> parsedJobLinkList = await _robotaUaJobLinkParser.GetJobLinksFromSearchLink(_searchlink, _maxPages, cancellationToken);
        List<JobCard> parsedJobDetailsList = await _robotaUaJobDetailsParser.GetJobDetailsList(parsedJobLinkList, cancellationToken);

        return parsedJobDetailsList;
    }
}
