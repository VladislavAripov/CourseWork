namespace YouTrackIntegration.Model
{
    public class WorkItem
    {
        public string text { get; set; }
        public Duration duration { get; set; }
    }

    public class Duration
    {
        public int minutes { get; set; }
    }
}