using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Importer.Jira.Fields
{
    public class Assignee
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
