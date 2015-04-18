using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Importer.YouTrack
{
    [XmlRoot("issues")]
    [Serializable]
    public class Issues
    {
        [XmlElement("issue")]
        public List<Issue> Issue;

        public Issues()
        {
            Issue = new List<Issue>();
        }
    }
}
