namespace YouTrackIntegration.Model
{
    public class ClockifyYouTrackAssociation
    {
        public string clockifyId { get; set; }
        
        public YouTrack[] youTracks { get; set; }

        public bool IsValid()
        {
            return clockifyId != null && youTracks != null;
        }
    }
}