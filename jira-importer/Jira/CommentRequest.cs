using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Importer.Jira
{
    public class CommentRequest
    {
        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
