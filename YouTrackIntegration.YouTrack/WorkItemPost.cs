namespace YouTrackIntegration.YouTrack
{
    public class WorkItemPost
    {
        public WorkItemPost(string text, int minutes, string id)
        {
            this.text = text;
            duration = new Duration(minutes);
            author = new WorkItemAuthor(id);
        }

        public string text { get; set; }
        public Duration duration { get; set; }
        
        public WorkItemAuthor author { get; set; }
        
        
        public class Duration
        {
            public Duration(int minutes)
            {
                this.minutes = minutes;
            }
            public int minutes { get; set; }
        }
    }
}