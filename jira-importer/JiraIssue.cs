using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace jira_importer
{
	[JsonObject("fields")]
	public class JiraIssue
	{
		[JsonProperty("project")]
		public JiraProject JiraProject { get; set; }

		[JsonProperty("summary")]
		public string Summary { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("issuetype")]
		public JiraIssueType JiraIssueType { get; set; }

		[JsonProperty("customfield_12000")]
		public string DefectId { get; set; }

		public JiraIssue()
		{
			this.JiraProject = new JiraProject();
		}
	}

	public class JiraProject
	{
		[JsonProperty("key")]
		public string Key { get; set; }
	}

	public class JiraIssueType
	{
		private JiraIssueType(string value) { Name = value; }

		public static JiraIssueType Bug { get { return new JiraIssueType("Bug"); } }
		public static JiraIssueType Defect { get { return new JiraIssueType("Defect"); } }
		public static JiraIssueType Task { get { return new JiraIssueType("Task"); } }
		public static JiraIssueType Improvement { get { return new JiraIssueType("Improvement"); } }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
