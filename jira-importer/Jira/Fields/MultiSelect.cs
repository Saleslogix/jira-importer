﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Importer.Jira.Fields
{
    public class MultiSelect
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
