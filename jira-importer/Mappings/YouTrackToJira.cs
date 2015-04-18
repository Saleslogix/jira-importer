using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer.Mappings
{
    public static class YouTrackToJira
    {
        public static Dictionary<string, Jira.IssueType> IssueType
        {
            get 
            {
                return new Dictionary<string, Jira.IssueType>
                {
                    {
                        "Bug", Jira.IssueType.Bug
                    },
                    {
                        "Cosmetics", Jira.IssueType.Task
                    },
                    {
                        "Feature", Jira.IssueType.NewFeature
                    },
                    {
                        "Task", Jira.IssueType.Task
                    },
                    {
                        "Usability Problem", Jira.IssueType.Task
                    },
                    {
                        "Story", Jira.IssueType.UseCase
                    },
                    {
                        "Performance Problem", Jira.IssueType.Defect
                    }
                };
            }
        }


        public static Dictionary<string, string> Priority
        {
            get {
                return new Dictionary<string, string>
                {
                    {
                        "Show-stopper", "Blocker"
                    },
                    {
                        "Critical", "Critical"
                    },
                    {
                        "Major", "Major"
                    },
                    {
                        "Normal", "Minor"
                    },
                    {
                        "Minor", "Trivial"
                    },
                    {
                        "P5 - Minor", "Minor" 
                    },
                    {
                        "P4- Normal", "Minor" 
                    },
                    {
                        "P3 - Major", "Major" 
                    },
                    {
                        "P2 - Critical", "Critical" 
                    },
                    {
                        "P1 - Show-stopper", "Blocker"
                    }
                };
            }
        }

        public static Dictionary<string, Action<YouTrack.Field, Jira.IssueRequest>> Properties 
        {
            get
            {
                return new Dictionary<string, Action<YouTrack.Field, Jira.IssueRequest>>
                {
                    {
                        "numberInProject", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.DefectId = request.ProjectId + "-" + field.Value;
                        }
                    },
                    {
                        "description", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Description = field.Value;
                        }
                    },
                    {
                        "summary", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Summary = field.Value;
                        }
                    },
                    {
                        "Type", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.JiraIssueType = Mappings.YouTrackToJira.IssueType[field.Value];
                        }
                    },
                    {
                        "Priority", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Priority = Mappings.YouTrackToJira.Priority[field.Value];
                        }
                    }
                };
            }
        }
    }
}