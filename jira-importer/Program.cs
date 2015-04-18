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

                    //var json = JsonConvert.SerializeObject(jiraIssue, Formatting.Indented);
                    //Console.WriteLine(json);

                    var jsonOutDir = Path.Combine(file.DirectoryName, projectId);
                    Directory.CreateDirectory(jsonOutDir);
                    using (var json = File.CreateText(Path.Combine(jsonOutDir, string.Format("{0}.json", jiraIssue.DefectId))))
                    {
                        var serializer = new JsonSerializer { Formatting = Formatting.Indented };
                        serializer.Serialize(json, jiraIssue);
                    }

                    /*Jira.IssueResponse response = await createNewJiraIssue(jiraIssue);
                    if (response != null)
                    {
                        Console.WriteLine("New issue created with ID: " + response.Id);
                    }*/
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
                HttpResponseMessage response = await client.PutAsJsonAsync<Jira.IssueRequest>("issue", issueRequest);

                if (response.IsSuccessStatusCode)
                {
                    Jira.IssueResponse results = await response.Content.ReadAsAsync<Jira.IssueResponse>();
                    return results;
                }
            }

            return null;
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
