using Newtonsoft.Json;

namespace Importer.Jira
{
    public class IssueType
    {
        private IssueType(string value) { Name = value; }

        public static IssueType Bug { get { return new IssueType("Bug"); } }
        public static IssueType Defect { get { return new IssueType("Defect"); } }
        public static IssueType Task { get { return new IssueType("Task"); } }
        public static IssueType Improvement { get { return new IssueType("Improvement"); } }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
