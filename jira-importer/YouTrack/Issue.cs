using System.Collections.Generic;
using System.Xml.Serialization;

namespace Importer.YouTrack
{
    public class Issue
    {
        [XmlElement("field")]
        public List<Field> Fields { get; set; }

        [XmlElement("comment")]
        public List<Comment> Comments { get; set; }

        public Issue()
        {
            Fields = new List<Field>();
        }
    }
}
