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
    public class ClockifyYouTrackUpdateController : ControllerBase
    {
        private const string JsonPath = "Data/ClockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackAssociation> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackUpdateController> _logger;

        public ClockifyYouTrackUpdateController(ILogger<ClockifyYouTrackUpdateController> logger)
        {
            _userAssociations = JsonSerializer.Deserialize<ClockifyYouTrackAssociation[]>(_userAssociationsJson)
                ?.ToList();
            _logger = logger;
        }

        
        
        [HttpPost]
        public void UpdateTask(ClockifyApiModel request)
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
                    // Если отметка времени не была найдена - останавливаем выполнение метода.
                    if (item == null)
                        return;
                    
                    url = $"{association.youTrack.domain}/api/issues/{issueId}/timeTracking/workItems/{item?.id}";

                    var workItem = new WorkItemPost()
                    {
                        duration = new Duration() {minutes = GetSpentTime(request)},
                        text = ChangeUpdateName(item?.text, request.user.name)
                    };
                    var workItemJson = JsonSerializer.Serialize(workItem);
                    var workItemBytes = Encoding.GetEncoding("utf-8").GetBytes(workItemJson);
                    
                    webClient.Headers.Add("Content-Type", "application/json");
                    webClient.UploadData(url, workItemBytes);
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

        private string ChangeUpdateName(string workItemText, string newUpdateName)
        {
            if (workItemText == null)
                return "";
            
            // Знаем, что поле апдейт последнее - ищем вторую
            // с конца ковычку и удаляем старое значение
            var i = workItemText.Length - 2;
            while (i > 0 && workItemText[i] != '"')
                i--;

            workItemText = workItemText.Remove(i);
            workItemText += '"' + newUpdateName + '"';
            
            return workItemText;
        }
        
        private static readonly Encoding encoding = Encoding.UTF8;
        
        public static string Decrypt(byte[] data)
        {
            return encoding.GetString(data);
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