using System.Collections.Generic;
using Importer.Jira.Fields;
using Newtonsoft.Json;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Formatting = Newtonsoft.Json.Formatting;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = XmlReader.Create("MBL.xml"); // TODO: Don't hardcode

            var s = new XmlSerializer(typeof(YouTrack.Issues));
            var issues = (YouTrack.Issues)s.Deserialize(reader);
            var youTrackToJiraTypeMapping = new Dictionary<string, Jira.IssueType> // TODO: Move
            {
                {
                    "Bug", Jira.IssueType.Bug
                },
                {
                    "Cosmetics", Jira.IssueType.Task
                },
                {
                    "Feature", Jira.IssueType.NewFeature
                },
                {
                    "Task", Jira.IssueType.Task
                },
                {
                    "Usability Problem", Jira.IssueType.Task
                }
            };

            var youTrackToJiraPriorityMapping = new Dictionary<string, string> // TODO: Move
            {
                {
                    "Show-stopper", "Blocker"
                },
                {
                    "Critical", "Critical"
                },
                {
                    "Major", "Major"
                },
                {
                    "Normal", "Minor"
                },
                {
                    "Minor", "Trivial"
                }
            };

            var mapping = new Dictionary<string, Action<YouTrack.Field, Jira.IssueRequest>> // TODO: Move
            {
                {
                    "numberInProject", delegate(YouTrack.Field field, Jira.IssueRequest request)
                    {
                        request.DefectId = "MBL-" + field.Value;
                    }
                },
                {
                    "description", delegate(YouTrack.Field field, Jira.IssueRequest request)
                    {
                        request.Description = field.Value;
                    }
                },
                {
                    "summary", delegate(YouTrack.Field field, Jira.IssueRequest request)
                    {
                        request.Summary = field.Value;
                    }
                },
                {
                    "Type", delegate(YouTrack.Field field, Jira.IssueRequest request)
                    {
                        request.JiraIssueType = youTrackToJiraTypeMapping[field.Value];
                    }
                },
                {
                    "Priority", delegate(YouTrack.Field field, Jira.IssueRequest request)
                    {
                        request.Priority = youTrackToJiraPriorityMapping[field.Value];
                    }
                }
            };

            foreach (var issue in issues.Issue)
            {
                var jiraIssue = new Jira.IssueRequest
                {
                    JiraProjectRequest = { Key = "INFORCRM" }, // TODO: Don't hardcode
                    JiraIssueType = Jira.IssueType.Bug,
                    Description = string.Empty,
                    Summary = string.Empty
                };

                jiraIssue.Versions.Add(new MultiSelect { Value = "Mobile 3.3" }); // TODO: Don't hardcode

                foreach (var field in issue.Fields)
                {
                    Action<YouTrack.Field, Jira.IssueRequest> action;
                    if (mapping.TryGetValue(field.Name, out action))
                    {
                        action(field, jiraIssue);
                    }
                }

                //var json = JsonConvert.SerializeObject(jiraIssue, Formatting.Indented);
                //Console.WriteLine(json);

                using (var file = File.CreateText(jiraIssue.DefectId + ".json"))
                {
                    var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                    serializer.Serialize(file, jiraIssue);
                }
            }

            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
