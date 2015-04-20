using Newtonsoft.Json;

namespace Importer.Jira
{
    public class CommentRequest
    {
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
