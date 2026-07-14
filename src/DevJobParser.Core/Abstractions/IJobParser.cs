using DevJobParser.Core.DTO;

namespace DevJobParser.Core.Abstractions
{
    public interface IJobParser
    {
        string SourceName { get; }
        Task<List<JobCard>> GetJobCardList(CancellationToken cancellationToken);
    }
}