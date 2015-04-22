using System;
using System.Collections.Generic;
using Importer.Jira.Fields;

namespace Importer.Mappings
{
    /// <summary>
    /// Mappings for YouTrack to Jira datasets. The LHS (key) will be YouTrack, the RHS (value) will be Jira.
    /// </summary>
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

        public static Dictionary<string, string> Users
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {
                        "jbest", "jbest2"
                    },
                    {
                        "chenrie", "chenrie"
                    },
                    {
                        "kbratton", "klockyer-bratton"
                    },
                    {
                        "bdonison", "bdonison"
                    },
                    {
                        "mphipps", "mphipps"
                    },
                    {
                        "root", "jperona"
                    },
                    {
                        "default", "klockyer-bratton"
                    }
                };
            }
        }

        public static Dictionary<string, string> Versions
        {
            get
            {
                return new Dictionary<string, string>
                {
                    {
                        "Sprint 2", "Mobile 3.0"
                    },
                    {
                        "Sprint 3", "Mobile 3.0"
                    },
                    {
                        "Sprint 4 (3.0)", "Mobile 3.0"
                    },
                    {
                        "Sprint 5 (3.0.1 and 3.0.2)", "Mobile 3.0.2"
                    },
                    {
                        "Sprint 6 (3.0.3)", "Mobile 3.0.3"
                    },
                    {
                        "Sprint 7 (3.1)", "Mobile 3.1"
                    },
                    {
                        "Sprint 8 (3.0.4)", "Mobile 3.0.4"
                    },
                    {
                        "Sprint 9 (3.2)", "Mobile 3.2"
                    },
                    {
                        "Sprint 10 (3.1.1)", "Mobile 3.1.1"
                    },
                    {
                        "Sprint 11 (3.2)", "Mobile 3.2"
                    },
                    {
                        "Sprint 12 (3.2)", "Mobile 3.2"
                    },
                    {
                        "Sprint 13 (3.2)", "Mobile 3.2"
                    },
                    {
                        "Sprint 14 (3.3)", "Mobile 3.3"
                    },
                    {
                        "3.2.1", "Mobile 3.2.1"
                    },
                    {
                        "Sprint 15 (3.3)", "Mobile 3.3"
                    },
                    {
                        "Sprint 16 (3.3)", "Mobile 3.3"
                    },
                    {
                        "Sprint 17 (3.3)", "Mobile 3.3"
                    },
                    {
                        "Sprint 18 (3.3)", "Mobile 3.3"
                    },
                    {
                        "Sprint 19 (3.3)", "Mobile 3.3"
                    },
                    {
                        "default", "Backlog"
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
                            request.Fields.DefectId = request.Fields.ProjectId + "-" + field.Value;
                        }
                    },
                    {
                        "description", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Fields.Description = string.IsNullOrWhiteSpace(field.Value) ? "None" : field.Value;
                        }
                    },
                    {
                        "summary", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Fields.Summary = field.Value;
                        }
                    },
                    {
                        "Type", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Fields.JiraIssueType = IssueType[field.Value];
                        }
                    },
                    {
                        "Priority", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            request.Fields.Priority = new Priority{ Name = Priority[field.Value] };
                        }
                    },
                    {
                        "Assignee", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            string value;
                            const string defaultUser = "default";
                            if (Users.TryGetValue(field.Value ?? defaultUser, out value))
                            {
                                request.Fields.Assignee = new User { Name = value };
                            }
                            else
                            {
                                request.Fields.Assignee = new User { Name = Users[defaultUser] };
                            }
                        }
                    },
                    {
                        "Fix versions", delegate(YouTrack.Field field, Jira.IssueRequest request)
                        {
                            string value;
                            const string defaultVersion = "default";
                            
                            if (Versions.TryGetValue(field.Value ?? defaultVersion, out value))
                            {
                                request.Fields.FixVersions.Add(new VersionPicker
                                {
                                    Name = value
                                });
                            }
                            else
                            {
                                request.Fields.FixVersions.Add(new VersionPicker
                                {
                                    Name = Versions[defaultVersion]
                                });
                            }
                        }
                    }
                };
            }
        }
    }
}