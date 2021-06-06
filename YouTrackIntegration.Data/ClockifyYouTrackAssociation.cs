using System;

namespace YouTrackIntegration.Data
{
    public class ClockifyYouTrackAssociation
    {
        public ClockifyYouTrackAssociation(string userId, string workspaceId, string domain, string permToken)
        {
            this.userId = userId;
            this.workspaceId = workspaceId;
            this.domain = domain;
            this.permToken = permToken;
        }

        public string Id { get; set; }
        
        public string userId { get; set; }

        public string workspaceId { get; set; }

        public string domain { get; set; }

        public string permToken { get; set; }
        
        public string defaultIssueId { get; set; }

        public User[] users { get; set; }


        public bool IsValid()
        {
            return users != null;
        }
    }
}
