using System;
using System.Linq;
using System.Net.Http;
using YouTrackIntegration.Clockify;
using YouTrackIntegration.Data;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using YouTrackSharp;
using YouTrackSharp.TimeTracking;


namespace YouTrackIntegration.Services
{
    public class YouTrackService
    {
        #region Public API

        public void CreateWorkItemInIssue(ClockifyYouTrackAssociation association, ClockifyWebhookModel clockifyWebhook)
        {
            // Ищем от какого пользователя идет запрос, некая аутентификация и авторизация.
            var user = association.users.FirstOrDefault(u => u.clockifyUserId == clockifyWebhook.userId);
            if (user == null)
                return;

            // Создаем подключение к YouTrack.
            var connection = new BearerTokenConnection(association.domain, association.permToken);

            // Берем issueId из описания вебхука или берем
            // запасной issueId (пользовательский или основной).
            var issueId = GetIssueIdFromDescriptionOrDefaultIssueId(connection, clockifyWebhook, association, user);
            // Если найти issueId не удалось, завершаем метод -
            // мы не знаем куда добавлять отметку времени.
            if (string.IsNullOrEmpty(issueId)) 
                return;
            
            // Создаем отметку времени, которая будет отправлена в YouTrack.
            var workItem = CreateWorkItem(clockifyWebhook, user);

            // Отправляем отметку времени в конкретный issue в конкретном YouTrack.
            CreateWorkItemForIssue(connection, issueId, workItem);
        }

        public void UpdateWorkItemInIssue(ClockifyYouTrackAssociation association, ClockifyWebhookModel clockifyWebhook)
        {
            // Ищем от какого пользователя идет запрос, некая аутентификация и авторизация.
            var user = association.users.FirstOrDefault(u => u.clockifyUserId == clockifyWebhook.userId);
            if (user == null)
                return;

            // Создаем подключение к YouTrack.
            var connection = new BearerTokenConnection(association.domain, association.permToken);
            
            // Берем issueId из описания вебхука или берем
            // запасной issueId (пользовательский или основной).
            var issueId = GetIssueIdFromDescriptionOrDefaultIssueId(connection, clockifyWebhook, association, user);
            // Если найти issueId не удалось, завершаем метод -
            // мы не знаем где обновлять отметку времени.
            if (string.IsNullOrEmpty(issueId))
                return;

            // Получаем отметку времени из YouTrack и обновляем ее (локально),
            // если отметки времени нет, завершаем метод - обновлять нечего.
            var workItem = GetWorkItemFromIssue(connection, issueId, clockifyWebhook);
            if (workItem == null)
                return;
            UpdateWorkItem(workItem, clockifyWebhook);
            
            // Отправляем обновленную отметку времени в YouTrack.
            UpdateWorkItemForIssue(connection, issueId, workItem);
        }
        
        public void DeleteWorkItemInIssue(ClockifyYouTrackAssociation association, ClockifyWebhookModel clockifyWebhook)
        {
            // Ищем от какого пользователя идет запрос, некая аутентификация и авторизация.
            var user = association.users.FirstOrDefault(u => u.clockifyUserId == clockifyWebhook.userId);
            if (user == null)
                return;

            // Создаем подключение к YouTrack.
            var connection = new BearerTokenConnection(association.domain, association.permToken);

            // Берем issueId из описания вебхука или берем
            // запасной issueId (пользовательский или основной).
            var issueId = GetIssueIdFromDescriptionOrDefaultIssueId(connection, clockifyWebhook, association, user);
            // Если найти issueId не удалось, завершаем метод -
            // мы не знаем где удалять отметку времени.
            if (string.IsNullOrEmpty(issueId))
                return;

            // Получаем отметку времени из YouTrack, еслии ее нет, завершаем метод - удалять нечего.
            var workItem = GetWorkItemFromIssue(connection, issueId, clockifyWebhook);
            if (workItem == null)
                return;
            
            // Удаляем отметку времени в YouTrack.
            DeleteWorkItemForIssue(connection, issueId, workItem.Id);
        }

        public async Task<string> GetYouTrackUsers(string domain, string permToken)
        {
            // Работает через HttpClient, т.к. YouTrackSharp не предоставляет необходимый сервис.
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", permToken);

            var url = $"{domain}/api/users?fields=name,login";

            var usersJson = await httpClient.GetStringAsync(url);
                
            return usersJson;
        }

        #endregion
        
        
        

        #region Private methods

        #region Post data to YouTrack

        private void CreateWorkItemForIssue(BearerTokenConnection connection, string issueId, WorkItem workItem)
        {
            var timeTrackingService = connection.CreateTimeTrackingService();
            timeTrackingService.CreateWorkItemForIssue(issueId, workItem);
        }

        private void UpdateWorkItemForIssue(BearerTokenConnection connection, string issueId, WorkItem workItem)
        {
            var timeTrackingService = connection.CreateTimeTrackingService();
            timeTrackingService.UpdateWorkItemForIssue(issueId, workItem.Id, workItem);
        }

        private void DeleteWorkItemForIssue(BearerTokenConnection connection, string issueId, string workItemId)
        {
            var timeTrackingService = connection.CreateTimeTrackingService();
            timeTrackingService.DeleteWorkItemForIssue(issueId, workItemId);
        }

        #endregion
        
        
        
        #region WorkItem

        private WorkItem CreateWorkItem(ClockifyWebhookModel clockifyWebhook, User user)
        {
            // Задаем текущую дату и время.
            var dateTime = DateTime.Now;
            var duration = GetDuration(clockifyWebhook.timeInterval);
            // Описание отметки времени включает в себя описание и id отметки времени в clockify.
            var description = $"{clockifyWebhook.description}\n{clockifyWebhook.id}";
            var author = new Author {Login = user.youTrackUserLogin};

            return new WorkItem(dateTime, duration, description, null, author);
        }
        
        private void UpdateWorkItem(WorkItem workItem, ClockifyWebhookModel clockifyWebhook)
        {
            // При обновлении отметки времени в clockify
            // могут меняться и ее описание, и ее продолжительность.
            // Автор отметки времени не меняется.
            workItem.Description = $"{clockifyWebhook.description}\n{clockifyWebhook.id}";
            workItem.Duration = GetDuration(clockifyWebhook.timeInterval);
        }
        
        private TimeSpan GetDuration(TimeInterval timeInterval)
        {
            var start = timeInterval.start;
            var end = timeInterval.end;

            var spentTime = end - start;

            var spentDays = spentTime.Days;
            var spentHours = spentTime.Hours;
            var spentMinutes = spentTime.Minutes;
            spentMinutes += (spentTime.Seconds > 30) ? 1 : 0;

            return new TimeSpan(spentDays, spentHours, spentMinutes, 0);
        }
        
        private WorkItem GetWorkItemFromIssue(BearerTokenConnection connection, string issueId,
            ClockifyWebhookModel clockifyWebhook)
        {
            var timeTrackingService = connection.CreateTimeTrackingService();
            var workItems = timeTrackingService.GetWorkItemsForIssue(issueId).Result.ToList();
            var workItem = workItems.FirstOrDefault(item => IsTimeStampIdExists(item.Description, clockifyWebhook.id));

            return workItem;
        }
        
        private bool IsTimeStampIdExists(string workItemText, string timestampId)
        {
            if (workItemText == null)
                return false;

            return workItemText.Contains(timestampId);
        }

        #endregion

        
        
        #region IssueId

        private string GetIssueIdFromDescriptionOrDefaultIssueId(BearerTokenConnection connection,
            ClockifyWebhookModel clockifyWebhook, ClockifyYouTrackAssociation association, User user)
        {
            // Выделяем issueId из описания вебхука,
            // если в переданном нам YouTrack он есть - возвращаем его,
            // иначе смотрим на defaultIssueId.
            var issueId = GetYouTrackIssueId(clockifyWebhook.description);
            if (IsIssueExists(connection, issueId)) return issueId;

            // Смотрим defaultIssueId.
            if (!string.IsNullOrEmpty(user.defaultIssueId)) return user.defaultIssueId;
            if (!string.IsNullOrEmpty(association.defaultIssueId)) return association.defaultIssueId;

            // Если ничего не нашли возвращаем null.
            return null;
        }
        
        private bool IsIssueExists(BearerTokenConnection connection, string issueId)
        {
            var issueService = connection.CreateIssuesService();

            return issueService.Exists(issueId).Result;
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
                if (!char.IsDigit(description[i]))
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

        #endregion

        #endregion
    }
}