﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jira.Fields
{
    public class Components
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
