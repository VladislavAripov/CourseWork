namespace YouTrackIntegration.Clockify
{
    public class ClockifyWebhookModel
    {
        public string id { get; set; }
        public string userId { get; set; }
        public string workspaceId { get; set; }
        public string description { get; set; }
        public TimeInterval timeInterval { get; set; }
    }
}