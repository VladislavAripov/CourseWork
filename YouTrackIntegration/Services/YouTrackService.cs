using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using YouTrackIntegration.Clockify;
using YouTrackIntegration.YouTrack;
using YouTrackIntegration.Data;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace YouTrackIntegration.Services
{
    public class YouTrackService
    {
        public async void CreateWorkItemInIssue(ClockifyYouTrackConnection connection, ClockifyApiModel clockifyRequest)
        {
            var user = connection.users.FirstOrDefault(u => u.clockifyId == clockifyRequest.userId);
            if (user == null)
                return;

            using (var httClient = new HttpClient())
            {
                var issueId = GetYouTrackIssueId(clockifyRequest.description);

                var workItemText = (issueId == user.defaultIssueId)
                    ? $"{clockifyRequest.description}\n{clockifyRequest.id}"
                    : clockifyRequest.id;

                var workItem = new WorkItemPost(workItemText, GetSpentTime(clockifyRequest), user.youTrackId);
                var workItemJson = JsonSerializer.Serialize(workItem);

                var stringContent = new StringContent(workItemJson, Encoding.UTF8, "application/json");

                httClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", connection.permToken);

                var url = $"{connection.domain}/api/issues/{issueId}/timeTracking/workItems";

                var response = await httClient.PostAsync(url, stringContent);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    using (httClient)
                    {
                        workItem = new WorkItemPost($"{clockifyRequest.description}\n{clockifyRequest.id}",
                            GetSpentTime(clockifyRequest), user.youTrackId);
                        workItemJson = JsonSerializer.Serialize(workItem);

                        httClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", connection.permToken);

                        stringContent = new StringContent(workItemJson, Encoding.UTF8, "application/json");
                        url = $"{connection.domain}/api/issues/{user.defaultIssueId}/timeTracking/workItems";

                        await httClient.PostAsync(url, stringContent);
                    }
                }
            }
        }

        public async void UpdateWorkItemInIssue(ClockifyYouTrackConnection connection, ClockifyApiModel clockifyRequest)
        {
            using (var httpClient = new HttpClient())
            {
                var issueId = GetYouTrackIssueId(clockifyRequest.description);

                var item = GetWorkItemFromIssue(connection.domain, connection.permToken, issueId, clockifyRequest.id);

                var url = $"{connection.domain}/api/issues/{issueId}/timeTracking/workItems/{item.Result.id}";

                var user =  connection.users.FirstOrDefault(u => u.clockifyId == clockifyRequest.userId);
                if (user == null)
                    return;

                var workItem = new WorkItemPost(item.Result.text, GetSpentTime(clockifyRequest), user.youTrackId);
                var workItemJson = JsonSerializer.Serialize(workItem);

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", connection.permToken);
                var stringContent = new StringContent(workItemJson, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(url, stringContent);
                
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    using (var httpClient1 = new HttpClient())
                    {
                        item = GetWorkItemFromIssue(connection.domain, connection.permToken, user.defaultIssueId, clockifyRequest.id);
                        
                        
                        url = $"{connection.domain}/api/issues/{user.defaultIssueId}/timeTracking/workItems/{item.Result.id}";
                        
                        user =  connection.users.FirstOrDefault(u => u.clockifyId == clockifyRequest.userId);
                        if (user == null)
                            return;
                        
                        
                        workItem = new WorkItemPost(item.Result.text, GetSpentTime(clockifyRequest), user.youTrackId);
                        workItemJson = JsonSerializer.Serialize(workItem);

                        httpClient1.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", connection.permToken);
                        
                        stringContent = new StringContent(workItemJson, Encoding.UTF8, "application/json");

                        await httpClient1.PutAsync(url, stringContent);
                    }
                }
            }
        }

        public async void DeleteWorkItemInIssue(ClockifyYouTrackConnection connection, ClockifyApiModel clockifyRequest)
        {
            using (var httpClient = new HttpClient())
            {
                var user = connection.users.FirstOrDefault(u => u.clockifyId == clockifyRequest.userId);
                if (user == null)
                    return;
                
                var issueId = GetYouTrackIssueId(clockifyRequest.description);

                Task<WorkItemGet> task = GetWorkItemFromIssue(connection.domain, connection.permToken, issueId, clockifyRequest.id);
                
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", connection.permToken);
                
                var url = $"{connection.domain}/api/issues/{issueId}/timeTracking/workItems/{task.Result.id}";
                
                var response = await httpClient.DeleteAsync(url);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    user = connection.users.FirstOrDefault(u => u.clockifyId == clockifyRequest.userId);
                        if (user == null)
                            return;
                        
                        task = GetWorkItemFromIssue(connection.domain, connection.permToken, user.defaultIssueId, clockifyRequest.id);
                        
                        url = $"{connection.domain}/api/issues/{user.defaultIssueId}/timeTracking/workItems/{task.Result.id}";
                        await httpClient.DeleteAsync(url);
                }
            }
        }

        public async Task<string> GetYouTrackUsers(string domain, string permToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", permToken);

                var url = $"{domain}/api/users?fields=id,name";

                var response = await httpClient.GetStringAsync(url);
                
                var users = JsonSerializer.Deserialize<YouTrackUser[]>(response);

                return response;
            }
        }
        
        public string GetYouTrackIssueId(string description)
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

            if (issueKey.Length > 0 && issueNumber.Length > 0)
                return $"{issueKey}-{issueNumber}";

            return null;
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

        private async Task<WorkItemGet> GetWorkItemFromIssue(string domain, string permToken, string issueId, string timestampId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", permToken);
                    
                var url = $"{domain}/api/issues/{issueId}/timeTracking/workItems?fields=id,text";

                var response = await httpClient.GetStringAsync(url);
                var workItems = JsonSerializer.Deserialize<WorkItemGet[]>(response)?.ToList();
                var item = workItems?.FirstOrDefault(a => IsIdExist(a.text,timestampId));

                return item;
            }
        }
        
        private static readonly Encoding encoding = Encoding.UTF8;
        
        public static string Decrypt(byte[] data)
        {
            return encoding.GetString(data);
        }
        
        private bool IsIdExist(string workItemText, string timestampId)
        {
            if (workItemText == null)
                return false;

            return workItemText.Contains(timestampId);
        }
    }
}