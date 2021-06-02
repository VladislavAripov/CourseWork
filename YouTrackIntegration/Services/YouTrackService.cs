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
        #region public api

        public async void CreateWorkItemInIssue(ClockifyYouTrackAssociation association, ClockifyWebhookModel clockifyWebhook)
        {
            // Ищем от какого пользователя идет запрос, некая аутентификация и авторизация.
            var user = association.users.FirstOrDefault(u => u.clockifyUserId == clockifyWebhook.userId);
            if (user == null)
                return;

            using var httpClient = new HttpClient();
            SetHttpClientParams(httpClient, association.permToken);
            
            
            var issueId = GetYouTrackIssueId(clockifyWebhook.description);
            
            var defaultIssueWorkItemText = $"{clockifyWebhook.description}\n{clockifyWebhook.id}";
            
            var workItemText = (issueId == user.defaultIssueId)
                ? defaultIssueWorkItemText
                : clockifyWebhook.id;

            var spentTime = GetSpentTime(clockifyWebhook);
            
            
            var workItem = CreateWorkItem(workItemText, spentTime, user.youTrackUserId);
            
            var url = $"{association.domain}/api/issues/{issueId}/timeTracking/workItems";

            var response = await httpClient.PostAsync(url, workItem);

            // Если выделенный IssueId оказался неверным, добавляем отметку времени в defaultIssue
            if (response.StatusCode == HttpStatusCode.NotFound & user.defaultIssueId != default)
            {
                workItem = CreateWorkItem(defaultIssueWorkItemText, spentTime, user.youTrackUserId);
                url = $"{association.domain}/api/issues/{user.defaultIssueId}/timeTracking/workItems";

                await httpClient.PostAsync(url, workItem);
            }
        }

        public async void UpdateWorkItemInIssue(ClockifyYouTrackAssociation association, ClockifyWebhookModel clockifyWebhook)
        {
            // Ищем от какого пользователя идет запрос, некая авторизация.
            var user =  association.users.FirstOrDefault(u => u.clockifyUserId == clockifyWebhook.userId);
            if (user == null)
                return;
            
            using var httpClient = new HttpClient();
            SetHttpClientParams(httpClient, association.permToken);

            var issueId = GetYouTrackIssueId(clockifyWebhook.description);
            
            // Пытаемся найти указанную запись в Issue с выделенным IssueId.
            var outdatedWorkItem = GetWorkItemFromIssue(association.domain, association.permToken, issueId, 
                clockifyWebhook.id);
            // Если запись не была найдена, пытаемся найти такую в default issue.
            if (outdatedWorkItem.Result == null & user.defaultIssueId != "")
            {
                outdatedWorkItem = GetWorkItemFromIssue(association.domain, association.permToken, user.defaultIssueId,
                    clockifyWebhook.id);
                // Если и в default issue такой записи нет, то прерываем выполненине метода - обновлять нечего.
                if (outdatedWorkItem.Result == null)
                    return;
                // Если запись была найдена в default issue, меняем текущий issueID на defaultIssueId.
                issueId = user.defaultIssueId;
            }
            
            var url = $"{association.domain}/api/issues/{issueId}/timeTracking/workItems/{outdatedWorkItem.Result?.id}";

            
            var spentTime = GetSpentTime(clockifyWebhook);
            var newWorkItemText = clockifyWebhook.id;
            
            var newWorkItem = CreateWorkItem(newWorkItemText, spentTime, user.youTrackUserId);
            
            
            await httpClient.PostAsync(url, newWorkItem);
        }

        public async void DeleteWorkItemInIssue(ClockifyYouTrackAssociation association, ClockifyWebhookModel clockifyWebhook)
        {
            // Ищем пользователя, от которого идет запрос, может понадобится его defaultIssueId.
            var user = association.users.FirstOrDefault(u => u.clockifyUserId == clockifyWebhook.userId);
            if (user == null)
                return;
           
            using var httpClient = new HttpClient();
            SetHttpClientParams(httpClient, association.permToken);
            
            var issueId = GetYouTrackIssueId(clockifyWebhook.description);

            // Пытаемся найти указанную запись в Issue с выделенным IssueId.
            var outdatedWorkItem = GetWorkItemFromIssue(association.domain, association.permToken, issueId, 
                clockifyWebhook.id);
            // Если запись не была найдена, пытаемся найти такую в default issue.
            if (outdatedWorkItem.Result == null & user.defaultIssueId != "")
            {
                outdatedWorkItem = GetWorkItemFromIssue(association.domain, association.permToken, user.defaultIssueId,
                    clockifyWebhook.id);
                // Если и в default issue такой записи нет, то прерываем выполненине метода - удалять нечего.
                if (outdatedWorkItem.Result == null)
                    return;
                // Если запись была найдена в default issue, меняем текущий issueID на defaultIssueId.
                issueId = user.defaultIssueId;
            }
            
            var url = $"{association.domain}/api/issues/{issueId}/timeTracking/workItems/{outdatedWorkItem.Result?.id}";
                
            await httpClient.DeleteAsync(url);
        }

        public async Task<string> GetYouTrackUsers(string domain, string permToken)
        {
            using var httpClient = new HttpClient();
            SetHttpClientParams(httpClient, permToken);

            var url = $"{domain}/api/users?fields=id,name";

            var usersJson = await httpClient.GetStringAsync(url);
                
            return usersJson;
        }

        #endregion

        
        

        #region private methods

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

            if (issueKey.Length > 0 && issueNumber.Length > 0)
                return $"{issueKey}-{issueNumber}";

            return null;
        }
        
        private bool IsCorrectSymbol(char c)
        {
            return Char.IsLetter(c) || Char.IsDigit(c) || (c == '_');
        }
        
        private int GetSpentTime(ClockifyWebhookModel webhook)
        {
            var start = webhook.timeInterval.start;
            var end = webhook.timeInterval.end;
            
            var spentTime = end - start;
            
            var spentHours = spentTime.Hours;
            var spentMinutes = spentTime.Minutes;
            spentMinutes += (spentTime.Seconds > 30) ? 1 : 0;

            return spentHours * 60 + spentMinutes;
        }

        private async Task<WorkItemGet> GetWorkItemFromIssue(string domain, string permToken, string issueId, string timestampId)
        {
            using var httpClient = new HttpClient();
            SetHttpClientParams(httpClient, permToken);
            
            var url = $"{domain}/api/issues/{issueId}/timeTracking/workItems?fields=id,text";

            var response = await httpClient.GetAsync(url);

            // Если запрос был выполнен неуспешно - мы не сможем выделить из него данные.
            if (response.StatusCode != HttpStatusCode.OK)
                return null;

            var workItemsStr = response.Content.ReadAsStringAsync();
            
            var workItems = JsonSerializer.Deserialize<WorkItemGet[]>(workItemsStr.Result)?.ToList();
            var workItem = workItems?.FirstOrDefault(item => IsIdExist(item.text, timestampId));

            return workItem;
        }

        private bool IsIdExist(string workItemText, string timestampId)
        {
            if (workItemText == null)
                return false;

            return workItemText.Contains(timestampId);
        }

        private StringContent CreateWorkItem(string workItemText, int minutes, string youTrackId)
        {
            var workItem = new WorkItemPost(workItemText, minutes, youTrackId);
            var workItemAsJson = JsonSerializer.Serialize(workItem);
            var workItemAsStringContent = 
                new StringContent(workItemAsJson, Encoding.UTF8, "application/json");

            return workItemAsStringContent;
        }

        private void SetHttpClientParams(HttpClient httpClient, string permToken)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", permToken);
        }
        
        #endregion
    }
}