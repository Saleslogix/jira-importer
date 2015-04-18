using System.Collections.Generic;
using Importer.Jira.Fields;
using Newtonsoft.Json;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Configuration;
using Formatting = Newtonsoft.Json.Formatting;

namespace Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = ConfigurationManager.AppSettings["YouTrackXMLDir"];
            var di = new DirectoryInfo(dir);
            if (!di.Exists)
            {
                di.Create();
                Console.WriteLine("Missing XML files in " + dir);
                Console.ReadLine();
                return;
            }

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
                },
                {
                    "Story", Jira.IssueType.UseCase
                },
                {
                    "Performance Problem", Jira.IssueType.Defect
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
                },
                {
                    "P5 - Minor", "Minor" 
                },
                {
                    "P4- Normal", "Minor" 
                },
                {
                    "P3 - Major", "Major" 
                },
                {
                    "P2 - Critical", "Critical" 
                },
                {
                    "P1 - Show-stopper", "Blocker"
                }
            };

            var mapping = new Dictionary<string, Action<YouTrack.Field, Jira.IssueRequest>> // TODO: Move
            {
                {
                    "numberInProject", delegate(YouTrack.Field field, Jira.IssueRequest request)
                    {
                        request.DefectId = request.ProjectId + "-" + field.Value;
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

            var files = di.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var reader = XmlReader.Create(file.FullName);
                var projectId = file.Name.Replace(file.Extension, string.Empty);

                var s = new XmlSerializer(typeof(YouTrack.Issues));
                var issues = (YouTrack.Issues)s.Deserialize(reader);

                foreach (var issue in issues.Issue)
                {
                    var jiraIssue = new Jira.IssueRequest
                    {
                        JiraProjectRequest = { Key = "INFORCRM" }, // TODO: Don't hardcode
                        JiraIssueType = Jira.IssueType.Bug,
                        Description = string.Empty,
                        Summary = string.Empty,
                        ProjectId = projectId
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

                    var jsonOutDir = file.DirectoryName + "\\" + projectId + "\\";
                    Directory.CreateDirectory(jsonOutDir);
                    using (var json = File.CreateText(jsonOutDir + jiraIssue.DefectId + ".json"))
                    {
                        var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                        serializer.Serialize(json, jiraIssue);
                    }
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
