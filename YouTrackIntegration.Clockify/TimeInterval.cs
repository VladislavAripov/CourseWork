using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTrackIntegration.Clockify
{
    public class TimeInterval
    {
        public DateTimeOffset start { get; set; }
        public DateTimeOffset end { get; set; }
        public string duration { get; set; }
    }
}
