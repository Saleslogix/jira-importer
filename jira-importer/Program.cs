using Newtonsoft.Json;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Importer
{
	class Program
	{
		static void Main(string[] args)
		{
			var reader = XmlReader.Create("MBL.xml");
			
			var s = new XmlSerializer(typeof(YouTrack.Issues));
			var issues = (YouTrack.Issues)s.Deserialize(reader);

			var jiraIssue = new Jira.IssueRequest
			{
			    JiraProjectRequest = {Key = "INFORCRM"},
			    Description = "foo",
			    Summary = "bar",
			    DefectId = "MBL-108049",
			    JiraIssueType = Jira.IssueType.Bug
			};

		    string json = JsonConvert.SerializeObject(jiraIssue, Newtonsoft.Json.Formatting.Indented);

			Console.WriteLine(json);

			Console.WriteLine();
			Console.ReadLine();
		}
	}
}
