namespace YouTrackIntegration.YouTrack
{
    public class WorkItemUpdate
    {
        public WorkItemUpdate(string text, int minutes, string id)
        {
            this.text = text;
            duration = new Duration(minutes);
            creator = new WorkItemCreator(id);
        }

        public string text { get; set; }
        public Duration duration { get; set; }
        
        public WorkItemCreator creator { get; set; }
    }
}