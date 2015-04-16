using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace jira_importer
{
	class Program
	{
		static void Main(string[] args)
		{
			var reader = XmlReader.Create("MBL.xml");
			
			var s = new XmlSerializer(typeof(YouTrackIssues));
			YouTrackIssues issues = (YouTrackIssues)s.Deserialize(reader);

			var jiraIssue = new JiraIssue();
			jiraIssue.JiraProject.Key = "INFORCRM";
			jiraIssue.Description = "foo";
			jiraIssue.Summary = "bar";
			jiraIssue.DefectId = "MBL-108049";
			jiraIssue.JiraIssueType = JiraIssueType.Bug;

			string json = JsonConvert.SerializeObject(jiraIssue, Newtonsoft.Json.Formatting.Indented);

			Console.WriteLine(json);

			Console.WriteLine();
			Console.ReadLine();
		}
	}
}
