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
        private const string JsonPath = "Data/ClockifyYouTrackAssociations.json";
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
        public void DeleteTask(ClockifyApiModel request)
        {
            var association = _userAssociations.Find(a => a.workspaceId == request.workspaceId);
            if (association != null)
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("Accept","application/json");
                    webClient.Headers.Add("Authorization", $"Bearer {association.youTrack.permToken}");
                    
                    var issueId = GetYouTrackIssueId(request.description);
                    var url = $"{association.youTrack.domain}/api/issues/{issueId}/timeTracking/workItems?fields=id,text";

                    var data = webClient.DownloadData(url);

                    var dataJson = Decrypt(data);
                    var workItems = JsonSerializer.Deserialize<WorkItemGet[]>(dataJson)?.ToList();
                    var item = workItems?.FirstOrDefault(a => ReadTimestampId(a.text) == request.id);

                    url = $"{association.youTrack.domain}/api/issues/{issueId}/timeTracking/workItems/{item?.id}";
                    webClient.UploadString(url, "DELETE", "");
                }
            }
        }

        
        private string ReadTimestampId(string workItemText)
        {
            if (workItemText == null)
                return "";
            
            var timestampId = "";

            var i = 0;
            // Надо прочитать id в ковычках - ищем первую ковычку.
            while (i < workItemText.Length && workItemText[i] != '"')
                i++;
            i++;
            
            // Читаем текст в ковычках.
            while (i < workItemText.Length && workItemText[i] != '"')
            {
                timestampId += workItemText[i];
                i++;
            }
            
            return timestampId;
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

        private static readonly Encoding encoding = Encoding.UTF8;
        
        public static string Decrypt(byte[] data)
        {
            return encoding.GetString(data);
        }
    }
}