using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YouTrackIntegration.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClockifyYouTrackCreateController : ControllerBase
    {
        private const string JsonPath = "Data/ClockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackAssociation> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackCreateController> _logger;

        public ClockifyYouTrackCreateController(ILogger<ClockifyYouTrackCreateController> logger)
        {
            _userAssociations = JsonSerializer.Deserialize<ClockifyYouTrackAssociation[]>(_userAssociationsJson)
                ?.ToList();
            _logger = logger;
        }
        

        [HttpPost]
        public void AddSpentTimeToTask(ClockifyApiModel request)
        {
            var association = _userAssociations.Find(a => a.workspaceId == request.workspaceId);
            if (association != null)
            {
                var workItemText = $"timestampId:\"{request.id}\"\n";
                workItemText += $"create:\"{request.user.name}\"\n";
                workItemText += $"update:\"{request.user.name}\"";
                
                var workItem = new WorkItemPost
                {
                    text = workItemText, duration = new Duration
                    {
                        minutes = GetSpentTime(request)
                    }
                };
                var workItemJson = JsonSerializer.Serialize(workItem);

                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept","application/json");
                    webClient.Headers.Add("Authorization", $"Bearer {association.youTrack.permToken}");
                    webClient.Headers.Add("Content-Type","application/json");
                        
                    var issueId = GetYouTrackIssueId(request.description);
                    var url = $"{association.youTrack.domain}/api/issues/{issueId}/timeTracking/workItems";
                        
                    var data = Encoding.GetEncoding("utf-8").GetBytes(workItemJson);
                        
                    webClient.UploadData(url, data);
                }
            }
        }
        

        private string GetYouTrackIssueId(string description)
        {
            var issueKey = "";
            foreach (var c in description)
            {
                if (!IsCorrectSymbol(c))
                    break;

                issueKey += c;
            }
            
            var issueNumber = "";
            for (var i = $"{issueKey}-".Length; i < description.Length; i++)
            {
                if (!Char.IsDigit(description[i]))
                    break;

                issueNumber += description[i];
            }
            
            return $"{issueKey}-{issueNumber}";
        }

        private bool IsCorrectSymbol(char c)
        {
            return Char.IsLetter(c) || Char.IsDigit(c) || (c == '_');
        }

        private int GetSpentTime(ClockifyApiModel request)
        {
            var start = request.timeInterval.start;
            var end = request.timeInterval.end;
            
            var spentTime = end - start;
            
            var spentHours = spentTime.Hours;
            var spentMinutes = spentTime.Minutes;
            spentMinutes += (spentTime.Seconds > 30) ? 1 : 0;

            return spentHours * 60 + spentMinutes;
        }
    }
}