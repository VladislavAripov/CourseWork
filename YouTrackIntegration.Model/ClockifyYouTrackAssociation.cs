namespace YouTrackIntegration.Model
{
    public class ClockifyYouTrackAssociation
    {
        public string userId { get; set; }
        
        public string workspaceId { get; set; }
        
        public YouTrack youTrack { get; set; }

        public bool IsValid()
        {
            return workspaceId != null && youTrack.IsValid();
        }
    }
}