using DevJobParser.Core.Abstractions;
using DevJobParser.Core.DTO;
using DevJobParser.Parsers.WorkUa.DetailsParser;
using DevJobParser.Parsers.WorkUa.LinkParser;
using DevJobParser.Parsers.WorkUa.Options;
using Microsoft.Extensions.Options;

namespace DevJobParser.Parsers.WorkUa
{
    public class WorkUaJobParser : IJobParser
    {
        private readonly WorkUaJobLinkParser _workUaJobLinkParser;
        private readonly WorkUaJobDetailsParser _workUaJobDetailsParser;
        private readonly string _searchlink;
        private readonly int _maxPages;
        public string SourceName {get; private set;}

        public WorkUaJobParser(
            WorkUaJobLinkParser workUaJobLinkParser,
            WorkUaJobDetailsParser workUaJobDetailsParser,
            IOptions<WorkUaParserOptions> options)
        {
            _workUaJobLinkParser = workUaJobLinkParser;
            _workUaJobDetailsParser = workUaJobDetailsParser;
            _searchlink = options.Value.SearchLink;
            _maxPages = options.Value.MaxPages;
            SourceName = options.Value.SourceName;
        }

        public async Task<List<JobCard>> GetJobCardList(CancellationToken cancellationToken)
        {
            List<string> parsedJobLinkList = await _workUaJobLinkParser.GetJobLinksFromSearchLink(_searchlink, _maxPages, cancellationToken);
            List<JobCard> parsedJobDetailsList = await _workUaJobDetailsParser.GetJobDetailsList(parsedJobLinkList, cancellationToken);

            return parsedJobDetailsList;
        }
    }
}