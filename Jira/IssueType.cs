using Newtonsoft.Json;

namespace Jira
{
    public class IssueType
    {
        private IssueType(string value, string id, bool subtask = false)
        {
            Name = value;
            Id = id;
            IsSubTask = subtask;
        }

        /**
         * Pulled using curl on the rest api:
         * curl -D- -u <jira user> -X GET -H "Content-Type: application/json" http://jira.infor.com/rest/api/2/issue/createmeta
         */
        public static IssueType Bug { get { return new IssueType("Bug", "1"); } }
        public static IssueType NewFeature { get { return new IssueType("New Feature", "2"); } }
        public static IssueType Task { get { return new IssueType("Task", "3"); } }
        public static IssueType Improvement { get { return new IssueType("Improvement", "4"); } }
        public static IssueType SubTask { get { return new IssueType("Sub-task", "5", true); } }
        public static IssueType UseCase { get { return new IssueType("Use Case", "6"); } }
        public static IssueType KnowledgeBase { get { return new IssueType("Knowledge Base", "11"); } }
        public static IssueType Defect { get { return new IssueType("Defect", "14"); } }
        public static IssueType Documentation { get { return new IssueType("Documentation", "35"); } }
        public static IssueType Question { get { return new IssueType("Question", "38"); } }
        public static IssueType Epic { get { return new IssueType("Epic", "40"); } }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonIgnore]
        [JsonProperty("subtask")]
        public bool IsSubTask { get; set; }
    }
}
