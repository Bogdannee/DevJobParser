using System;
using System.Collections.Generic;
using System.Text;

namespace DevJobParser.DTO
{
    public class JobCard
    {
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string Company { get; set; }
        public string? Salary { get; set; }
        public required string Description { get; set; }

        public Dictionary<string, string?>? AdditionalDetails { get; set; }
        public JobCard() { }
    }
}
