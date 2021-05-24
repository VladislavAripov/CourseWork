using System;

namespace YouTrackIntegration.Model
{
    public class ClockifyApiModel
    {
        public string id { get; set; }
        public User user { get; set; }
        public string workspaceId { get; set; }
        public string description { get; set; }
        public TimeInterval timeInterval { get; set; }
    }
}