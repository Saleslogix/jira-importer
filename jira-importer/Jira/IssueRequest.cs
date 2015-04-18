﻿using System.Collections.Generic;
using Importer.Jira.Fields;
using Newtonsoft.Json;

namespace Importer.Jira
{
    [JsonObject("fields")]
    public class IssueRequest
    {
        [JsonProperty("project")]
        public ProjectRequest JiraProjectRequest { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; }

        [JsonProperty("issuetype")]
        public IssueType JiraIssueType { get; set; }

        [JsonProperty("customfield_12000")]
        public string DefectId { get; set; }

        [JsonProperty("versions")]
        public List<MultiSelect> Versions { get; set; }

        [JsonIgnore]
        public string ProjectId { get; set; }

        public IssueRequest()
        {
            JiraProjectRequest = new ProjectRequest();
            Versions = new List<MultiSelect>();
        }
    }

}