namespace YouTrackIntegration.Model
{
    public class WorkItemPost
    {
        public string text { get; set; }
        public Duration duration { get; set; }
    }

    public class Duration
    {
        public int minutes { get; set; }
    }
}