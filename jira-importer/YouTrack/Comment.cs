﻿using System.Xml.Serialization;

namespace Importer.YouTrack
{
    public class Comment
    {
        [XmlAttribute("author")]
        public string Author { get; set; }

        [XmlAttribute("text")]
        public string Text { get; set; }

        [XmlAttribute("created")]
        public string Created { get; set; }
    }
}
