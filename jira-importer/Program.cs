using Importer.Jira.Fields;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace Importer
{
    class Program
    {
        static string jiraUserName;
        static string jiraPassword;

        static void Main(string[] args)
        {
            var xmlDir = ConfigurationManager.AppSettings["YouTrackXMLDir"];
            var xmlDirInfo = new DirectoryInfo(xmlDir);

            // Check for the XML directory, which is where the exported XML file(s) from YouTrack go.
            if (!xmlDirInfo.Exists)
            {
                xmlDirInfo.Create();
                Console.WriteLine("Missing XML files in " + xmlDir);
                Console.ReadLine();
                return;
            }

            Console.Write("Jira Username: ");
            jiraUserName = Console.ReadLine();

            Console.Write("\nJira Password: ");
            jiraPassword = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(jiraUserName) || string.IsNullOrWhiteSpace(jiraPassword))
            {
                Console.WriteLine("You must enter a Jira username and password.");
                Console.ReadLine();
                return;
            }

            Jira.Client.SetCredentials(jiraUserName, jiraPassword);

            var youtrackExportFiles = xmlDirInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
            foreach (var youtrackExportFile in youtrackExportFiles)
            {
                var reader = XmlReader.Create(youtrackExportFile.FullName);
                var projectId = youtrackExportFile.Name.Replace(youtrackExportFile.Extension, string.Empty);

                var xmlSerializer = new XmlSerializer(typeof(YouTrack.Issues));
                var youtrackIssues = (YouTrack.Issues)xmlSerializer.Deserialize(reader);
                var pending = new List<Task<Jira.IssueResponse>>();

                foreach (YouTrack.Issue youtrackIssue in youtrackIssues.Issue)
                {
                    var jiraIssue = new Jira.IssueRequest
                    {
                        Fields =
                        {
                            JiraProjectRequest = { Key = "INFORCRM" }, // TODO: Don't hardcode
                            JiraIssueType = Jira.IssueType.Bug,
                            Description = "None",
                            Summary = "None",
                            ProjectId = projectId
                        }

                    };

                    jiraIssue.Fields.Versions.Add(new VersionPicker { Name = "Mobile 3.3" }); // TODO: Don't hardcode
                    jiraIssue.Fields.Components.Add(new Components { Name = "Mobile" }); // TODO: Don't hardcode

                    foreach (var youtrackIssueField in youtrackIssue.Fields)
                    {
                        Action<YouTrack.Field, Jira.IssueRequest> propertyAction;
                        if (Mappings.YouTrackToJira.Properties.TryGetValue(youtrackIssueField.Name, out propertyAction))
                        {
                            propertyAction(youtrackIssueField, jiraIssue);
                        }
                    }

                    // Write out the issue payloads
                    var jsonOutDir = Path.Combine(youtrackExportFile.DirectoryName, projectId);
                    Directory.CreateDirectory(jsonOutDir);
                    using (var json = File.CreateText(Path.Combine(jsonOutDir, string.Format("{0}.json", jiraIssue.Fields.DefectId))))
                    {
                        var jsonSerializer = new JsonSerializer { Formatting = Formatting.Indented };
                        jsonSerializer.Serialize(json, jiraIssue);
                    }

                    var comments = youtrackIssue.Comments.Select(c => new Jira.CommentRequest { Body = string.Format("YouTrack Author: {0}\r\n{1}", c.Author, c.Text) }).ToArray();

                    // Write out the comments payloads
                    for (int i = 0; i < comments.Length; i++)
                    {
                        var c = comments[i];
                        using (var commentJson = File.CreateText(Path.Combine(jsonOutDir, string.Format("{0}-comment-{1}.json", jiraIssue.Fields.DefectId, i))))
                        {
                            var jsonSerializer = new JsonSerializer { Formatting = Formatting.Indented };
                            jsonSerializer.Serialize(commentJson, c);
                        }
                    }

                    pending.Add(Jira.Client.CreateNewJiraIssue(jiraIssue, comments));

                    // Prevent slamming the server with thousands of new issues + tens of comments per issue
                    System.Threading.Thread.Sleep(1000);
                }

                try
                {
                    Task.WaitAll(pending.ToArray());
                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Exceptions thrown when creating issues.");
                    foreach (var inner in e.InnerExceptions)
                    {
                        Console.WriteLine(inner.ToString());
                    }
                }
            }

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
