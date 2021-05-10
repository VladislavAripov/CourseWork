using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YouTrackIntegration.Model;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClockifyYouTrackDeleteController : ControllerBase
    {
        private const string JsonPath = "Data/clockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackAssociation> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackDeleteController> _logger;

        public ClockifyYouTrackDeleteController(ILogger<ClockifyYouTrackDeleteController> logger)
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
                var youTrack = FindYouTrack(association, request.description);
                if (youTrack != null)
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.Headers.Add("Accept","application/json");
                        webClient.Headers.Add("Authorization", $"Bearer {youTrack.youTrackPermToken}");
                        
                        var taskId = GetYouTrackTaskId(request.description, youTrack.taskKey);
                        var url = $"{youTrack.youTrackDomain}/api/issues/{taskId}/timeTracking/workItems?fields=id,text";

                        var data = webClient.DownloadData(url);

                        var dataJson = Decrypt(data);
                        var workItems = JsonSerializer.Deserialize<WorkItemGet[]>(dataJson)?.ToList();
                        var item = workItems?.FirstOrDefault(a => a.text == request.id);

                        url = $"{youTrack.youTrackDomain}/api/issues/{taskId}/timeTracking/workItems/{item?.id}";
                        webClient.UploadString(url, "DELETE", "");
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
        
        private static readonly Encoding encoding = Encoding.UTF8;
        
        public static string Decrypt(byte[] data)
        {
            return encoding.GetString(data);
        }
    }
}