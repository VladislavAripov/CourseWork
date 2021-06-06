namespace YouTrackIntegration.Data
{
    public class ClockifyYouTrackAssociation
    {
        public int Id { get; set; }

        public string workspaceId { get; set; }

        public string domain { get; set; }

        public string permToken { get; set; }
        
        public string defaultIssueId { get; set; }


        public ClockifyYouTrackAssociation(string workspaceId, string domain, string permToken, string defaultIssueId)
        {
            this.workspaceId = workspaceId;
            this.domain = domain;
            this.permToken = permToken;
            this.defaultIssueId = defaultIssueId;
        }

        public ClockifyYouTrackAssociation()
        {
        }
    }
}
