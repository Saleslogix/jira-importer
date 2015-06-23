using Newtonsoft.Json;

namespace Jira
{
    public class CommentRequest
    {
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
