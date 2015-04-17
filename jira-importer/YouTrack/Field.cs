using System.Xml.Serialization;

namespace Importer.YouTrack
{
    public class Field
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }
    }
}
