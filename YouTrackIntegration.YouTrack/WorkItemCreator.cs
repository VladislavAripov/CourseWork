﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTrackIntegration.YouTrack
{
    public class WorkItemCreator
    {
        public WorkItemCreator(string id)
        {
            this.id = id;
        }

        public string id { get; set; }
    }
}
