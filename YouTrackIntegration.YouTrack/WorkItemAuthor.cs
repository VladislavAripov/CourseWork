using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTrackIntegration.YouTrack
{
    public class WorkItemAuthor
    {
        public WorkItemAuthor(string id)
        {
            this.id = id;
        }

        public string id { get; set; }
    }
}
