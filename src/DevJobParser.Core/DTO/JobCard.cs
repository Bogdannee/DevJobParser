namespace DevJobParser.Core.DTO
{
    public class JobCard
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? Salary { get; set; }
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string?>? AdditionalDetails { get; set; }
    }
}
