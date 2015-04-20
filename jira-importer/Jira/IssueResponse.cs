using Newtonsoft.Json;

namespace Importer.Jira
{
    public class IssueResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }
    }
}
