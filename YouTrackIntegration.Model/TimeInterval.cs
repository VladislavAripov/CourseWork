using System;

namespace YouTrackIntegration.Model
{
    public class TimeInterval
    {
        public DateTimeOffset start { get; set; }
        public DateTimeOffset end { get; set; }
        public string duration { get; set; }
    }
}