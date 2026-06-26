using System;
using System.Collections.Generic;
using System.Text;

namespace DevJobParser.DTO
{
    public class JobCard
    {
        public string Url { get; init; }
        public string Title { get; init; }
        public string Company { get; init; }
        public string Salary { get; init; }
        public string Description { get; init; }

        public Dictionary<string, string> AdditionalDetails { get; init; }
    }
}
