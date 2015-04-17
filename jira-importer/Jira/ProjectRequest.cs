using Newtonsoft.Json;

namespace Importer.Jira
{
    public class ProjectRequest
    {
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}
