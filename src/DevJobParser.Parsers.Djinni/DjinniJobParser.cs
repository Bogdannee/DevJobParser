using DevJobParser.Core.Abstractions;
using DevJobParser.Core.DTO;
using DevJobParser.Parsers.Djinni.DetailsParser;
using DevJobParser.Parsers.Djinni.LinkParser;
using DevJobParser.Parsers.Djinni.Options;
using Microsoft.Extensions.Options;

namespace DevJobParser.Parsers.Djinni;

public class DjinniJobParser : IJobParser
{
    private readonly DjinniJobLinkParser _djinniJobLinkParser;
    private readonly DjinniJobDetailsParser _djinniJobDetailsParser;
    private readonly string _searchlink;
    private readonly int _maxPages;
    public string SourceName {get; private set;}

    public DjinniJobParser(
        DjinniJobLinkParser djinniJobLinkParser,
        DjinniJobDetailsParser djinniJobDetailsParser,
        IOptions<DjinniParserOptions> options)
    {
        _djinniJobLinkParser = djinniJobLinkParser;
        _djinniJobDetailsParser = djinniJobDetailsParser;
        _searchlink = options.Value.SearchLink;
        _maxPages = options.Value.MaxPages;
        SourceName = options.Value.SourceName;
    }

    public async Task<List<JobCard>> GetJobCardList(CancellationToken cancellationToken)
    {
        List<string> parsedJobLinkList = await _djinniJobLinkParser.GetJobLinksFromSearchLink(_searchlink, _maxPages, cancellationToken);
        List<JobCard> parsedJobDetailsList = await _djinniJobDetailsParser.GetJobDetailsList(parsedJobLinkList, cancellationToken);

        return parsedJobDetailsList;
    }
}