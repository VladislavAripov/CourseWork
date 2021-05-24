namespace YouTrackIntegration.Model
{
    public class YouTrack
    {
        public string domain { get; set; }
        
        public string permToken { get; set; }
        
        public string defaultIssueId { get; set; }

        public bool IsValid()
        {
            return domain != null && permToken != null;
        }
    }
}