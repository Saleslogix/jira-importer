using Importer.Jira.Fields;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
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

            if (!xmlDirInfo.Exists)
            {
                xmlDirInfo.Create();
                Console.WriteLine("Missing XML files in " + xmlDir);
                Console.ReadLine();
                return;
            }

            // Auth
            Console.WriteLine("Jira Username: ");
            jiraUserName = Console.ReadLine();

            Console.WriteLine("Jira Password: ");
            jiraPassword = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(jiraUserName) || string.IsNullOrWhiteSpace(jiraPassword))
            {
                Console.WriteLine("You must enter a Jira username and password.");
                Console.ReadLine();
                return;
            }

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
                        JiraProjectRequest = { Key = "INFORCRM" }, // TODO: Don't hardcode
                        JiraIssueType = Jira.IssueType.Bug,
                        Description = string.Empty,
                        Summary = string.Empty,
                        ProjectId = projectId
                    };

                    jiraIssue.Versions.Add(new MultiSelect { Value = "Mobile 3.3" }); // TODO: Don't hardcode

                    foreach (var youtrackIssueField in youtrackIssue.Fields)
                    {
                        Action<YouTrack.Field, Jira.IssueRequest> propertyAction;
                        if (Mappings.YouTrackToJira.Properties.TryGetValue(youtrackIssueField.Name, out propertyAction))
                        {
                            propertyAction(youtrackIssueField, jiraIssue);
                        }
                    }

                    // This is temp code to inspect the JSON before doing a real post,
                    // TODO: Remove me
                    var jsonOutDir = Path.Combine(youtrackExportFile.DirectoryName, projectId);
                    Directory.CreateDirectory(jsonOutDir);
                    using (var json = File.CreateText(Path.Combine(jsonOutDir, string.Format("{0}.json", jiraIssue.DefectId))))
                    {
                        var jsonSerializer = new JsonSerializer { Formatting = Formatting.Indented };
                        jsonSerializer.Serialize(json, jiraIssue);
                    }

                    IEnumerable<Jira.CommentRequest> comments = youtrackIssue.Comments.Select(c => new Jira.CommentRequest { Body = c.Text });
                    pending.Add(createNewJiraIssue(jiraIssue, comments));
                }

                Task.WaitAll(pending.ToArray());
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        // TODO: Move to a util class in the Jira folder?
        private static async Task<Jira.IssueResponse> createNewJiraIssue(Jira.IssueRequest issueRequest, IEnumerable<Jira.CommentRequest> comments)
        {
            using (var client = new HttpClient())
            {
                SetCommonHeaders(client);
                HttpResponseMessage response = await client.PostAsJsonAsync<Jira.IssueRequest>("issue", issueRequest);

                if (response.IsSuccessStatusCode)
                {
                    Jira.IssueResponse issueResults = await response.Content.ReadAsAsync<Jira.IssueResponse>();
                    Console.WriteLine(string.Format("New issue created with ID: {0}", issueResults.Id));
                    foreach (var comment in comments)
                    {
                        Task<HttpStatusCode> status = createJiraComment(issueResults.Id, comment);
                        var statusResult = status.Result; // Block to preserve order
                        if (statusResult == HttpStatusCode.OK)
                        {
                            Console.WriteLine(string.Format("Comment for {0} created.", issueResults.Id));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Failed to create comment for {0}.", issueResults.Id));
                        }
                    }
                    
                    return issueResults;
                }
                else
                {
                    return null;
                }
            }
        }

        private static async Task<HttpStatusCode> createJiraComment(string issueId, Jira.CommentRequest commentRequest)
        {
            using (var client = new HttpClient())
            {
                SetCommonHeaders(client);
                // Posting the comment doesn't return back a JSON response, just check the status code
                HttpResponseMessage response = await client.PostAsJsonAsync<Jira.CommentRequest>(string.Format("issue/{0}/comment", issueId), commentRequest);
                return response.StatusCode;
            }
        }

        private static void SetCommonHeaders (HttpClient client)
        {
            var baseUrl = ConfigurationManager.AppSettings["BaseURI"];
            var token = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", jiraUserName, jiraPassword));

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(token));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
