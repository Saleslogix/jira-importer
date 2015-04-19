using Importer.Jira.Fields;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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

            var files = xmlDirInfo.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
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
                        if (Mappings.YouTrackToJira.Properties.TryGetValue(field.Name, out action))
                        {
                            action(field, jiraIssue);
                        }
                    }

                    // This is temp code to inspect the JSON before doing a real post,
                    // TODO: Remove me
                    var jsonOutDir = Path.Combine(file.DirectoryName, projectId);
                    Directory.CreateDirectory(jsonOutDir);
                    using (var json = File.CreateText(Path.Combine(jsonOutDir, string.Format("{0}.json", jiraIssue.DefectId))))
                    {
                        var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                        serializer.Serialize(json, jiraIssue);
                    }

                    // This will be the production code that actually does the post
                    // TODO: Build up an IEnumerable<Task> list for creating an issue+comments?
                    Task<Jira.IssueResponse> response = createNewJiraIssue(jiraIssue);
                    var result = response.Result; // Blocking
                    if (result != null)
                    {
                        Console.WriteLine(string.Format("New issue created with ID: {0}", result.Id));
                        foreach (var comment in issue.Comments)
                        {
                            var commentRequest = new Jira.CommentRequest { Body = comment.Text };
                            Task<HttpStatusCode> status = createJiraComment(result.Id, commentRequest);
                            var results = status.Result; // Block to re-recreate the comments in-order
                            if (results == HttpStatusCode.OK)
                            {
                                Console.WriteLine(string.Format("Comment for {0} created.", result.Id));
                            }
                            else
                            {
                                Console.WriteLine(string.Format("Failed to create comment for {0}.", result.Id));
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        // TODO: Move to a util class in the Jira folder?
        private static async Task<Jira.IssueResponse> createNewJiraIssue(Jira.IssueRequest issueRequest)
        {
            using (var client = new HttpClient())
            {
                SetCommonHeaders(client);
                HttpResponseMessage response = await client.PostAsJsonAsync<Jira.IssueRequest>("issue", issueRequest);

                if (response.IsSuccessStatusCode)
                {
                    Jira.IssueResponse results = await response.Content.ReadAsAsync<Jira.IssueResponse>();
                    return results;
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
