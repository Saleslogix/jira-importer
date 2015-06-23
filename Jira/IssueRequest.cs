using Jira.Fields;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jira
{
    public class IssueRequest
    {
        [JsonProperty("fields")]
        public IssueFields Fields { get; set; }

        public IssueRequest()
        {
            Fields = new IssueFields();
        }
    }

    public class IssueFields
    {
        [JsonProperty("project")]
        public ProjectRequest JiraProjectRequest { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("priority")]
        public Priority Priority { get; set; }

        [JsonProperty("issuetype")]
        public IssueType JiraIssueType { get; set; }

        [JsonProperty("customfield_12000")]
        public string DefectId { get; set; }

        [JsonProperty("versions")]
        public List<VersionPicker> Versions { get; set; }

        [JsonProperty("fixVersions")]
        public List<VersionPicker> FixVersions { get; set; }

        [JsonProperty("assignee")]
        public User Assignee { get; set; }

        [JsonProperty("components")]
        public List<Components> Components { get; set; }

        [JsonIgnore]
        public string ProjectId { get; set; }

        public IssueFields()
        {
            JiraProjectRequest = new ProjectRequest();
            Versions = new List<VersionPicker>();
            FixVersions = new List<VersionPicker>();
            Components = new List<Components>();
        }
    }

}
