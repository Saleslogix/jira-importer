using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Importer.Jira
{
    public class Client
    {
        public static string JIRA_USER = "";
        public static string JIRA_PW = "";

        public static async Task<Jira.IssueResponse> CreateNewJiraIssue(Jira.IssueRequest issueRequest, Jira.CommentRequest[] comments)
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
                        Task<HttpStatusCode> status = Jira.Client.CreateJiraComment(issueResults.Id, comment);
                        var statusResult = status.Result; // Block to preserve comment order

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
                    Console.WriteLine(string.Format("Failed to create issue {0}.", issueRequest.Fields.DefectId));
                    return null;
                }
            }
        }

        public static async Task<HttpStatusCode> CreateJiraComment(string issueId, Jira.CommentRequest commentRequest)
        {
            using (var client = new HttpClient())
            {
                SetCommonHeaders(client);
                // Posting the comment doesn't return back a JSON response, just check the status code
                HttpResponseMessage response = await client.PostAsJsonAsync<Jira.CommentRequest>(string.Format("issue/{0}/comment", issueId), commentRequest);
                return response.StatusCode;
            }
        }

        public static void SetCredentials(string user, string pw)
        {
            JIRA_USER = user;
            JIRA_PW = pw;
        }

        private static void SetCommonHeaders(HttpClient client)
        {
            var baseUrl = ConfigurationManager.AppSettings["BaseURI"];
            var token = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", JIRA_USER, JIRA_PW));

            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(token));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
