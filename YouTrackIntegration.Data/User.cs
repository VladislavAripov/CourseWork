namespace YouTrackIntegration.Data
{
    public class User
    {
        public int Id { get; set; }
        public int ClockifyYouTrackAssociationId { get; set; }
        
        public string clockifyUserId { get; set; }
        
        public string youTrackUserLogin { get; set; }
        
        public string defaultIssueId { get; set; }
    }
}