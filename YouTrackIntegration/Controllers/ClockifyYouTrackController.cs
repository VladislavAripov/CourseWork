using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTrackIntegration.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClockifyYouTrackController : ControllerBase
    {
        private const string JsonPath = "Data/clockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackAssociation> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackController> _logger;

        public ClockifyYouTrackController(ILogger<ClockifyYouTrackController> logger)
        {
            _userAssociations = JsonSerializer.Deserialize<ClockifyYouTrackAssociation[]>(_userAssociationsJson)
                ?.ToList();
            _logger = logger;
        }

        
        
        [HttpPost]
        public void AddSpentTimeToTask(ClockifyApiModel request)
        {
            var association = _userAssociations.Find(a => a.clockifyId == request.userId);
            if (association != null)
            {
                var youTrack = FindYouTrack(association, request.description);
                if (youTrack != null)
                {
                    var start = request.timeInterval.start;
                    var end = request.timeInterval.end;
            
                    var spentTime = end - start;
            
                    var spentHours = spentTime.Hours;
                    var spentMinutes = spentTime.Minutes;
                    spentMinutes += (spentTime.Seconds > 30) ? 1 : 0;

                    var workItem = new WorkItem
                    {
                        text = request.id, duration = new Duration
                        {
                            minutes = spentHours * 60 + spentMinutes
                        }
                    };
                    var workItemJson = JsonSerializer.Serialize(workItem);

                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Accept","application/json");
                        webClient.Headers.Add("Authorization", $"Bearer {youTrack.youTrackPermToken}");
                        webClient.Headers.Add("Content-Type","application/json");
                        
                        var taskId = GetYouTrackTaskId(request.description, youTrack.taskKey);
                        var url = $"{youTrack.youTrackDomain}/api/issues/{taskId}/timeTracking/workItems";
                        
                        var data = Encoding.GetEncoding("utf-8").GetBytes(workItemJson);
                        
                        webClient.UploadData(url, data);
                    }
                }
            }
        }


        private Model.YouTrack FindYouTrack(ClockifyYouTrackAssociation association, string description)
        {
            foreach (var youTrack in association.youTracks)
                if (youTrack.taskKey + "-" == description.Substring(0, youTrack.taskKey.Length + 1))
                    return youTrack;

            return null;
        }

        private string GetYouTrackTaskId(string description, string taskKey)
        {
            var taskNumber = "";
            for (var i = taskKey.Length + 1; i < description.Length; i++)
            {
                if (!Char.IsDigit(description[i]))
                    break;
                taskNumber += description[i];
            }

            return $"{taskKey}-{taskNumber}";
        }
    }
}