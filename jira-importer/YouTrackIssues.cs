using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace jira_importer
{
	[XmlRoot("issues")]
	[Serializable]
	public class YouTrackIssues
	{
		[XmlElement("issue")]
		public List<Issue> Issue;

		public YouTrackIssues()
		{
			this.Issue = new List<Issue>();
		}
	}

	public class Issue
	{
		[XmlElement("field")]
		public List<Field> Fields { get; set; }

		[XmlElement("comment")]
		public List<Comment> Comments { get; set; }

		public Issue()
		{
			this.Fields = new List<Field>();
		}
	}

	public class Field
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("value")]
		public string Value { get; set; }
	}

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
