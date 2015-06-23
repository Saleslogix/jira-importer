using Newtonsoft.Json;

namespace Jira
{
    public class ProjectRequest
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
