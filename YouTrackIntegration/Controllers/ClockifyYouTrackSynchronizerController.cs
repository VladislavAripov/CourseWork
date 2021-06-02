using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YouTrackIntegration.Services;
using YouTrackIntegration.Clockify;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("api/synchronizer")]
    public class ClockifyYouTrackSynchronizerController : ControllerBase
    {
        private readonly AssociationsManager _associationsManager;
        private readonly YouTrackService _youTrackService;

        public ClockifyYouTrackSynchronizerController(AssociationsManager associationsManager, 
            YouTrackService youTrackService)
        {
            _youTrackService = youTrackService;
            _associationsManager = associationsManager;
        }
        
        
        [Route("create")]
        [HttpPost]
        public void AddWorkItemToIssue(ClockifyWebhookModel webhook)
        {
            var association = _associationsManager.GetAssociation(webhook.workspaceId);
            if (association != null)
                _youTrackService.CreateWorkItemInIssue(association, webhook);
        }
        
        
        [Route("update")]
        [HttpPost]
        public void UpdateTask(ClockifyWebhookModel webhook)
        {
            var association = _associationsManager.GetAssociation(webhook.workspaceId);
            if (association != null)
                _youTrackService.UpdateWorkItemInIssue(association, webhook);
        }


        [Route("delete")]
        [HttpPost]
        public void DeleteTask(ClockifyWebhookModel webhook)
        {
            var association = _associationsManager.GetAssociation(webhook.workspaceId);
            if (association != null)
                _youTrackService.DeleteWorkItemInIssue(association, webhook);
        }

        
        [Route("get_users")]
        [HttpGet]
        public Task<string> GetUsers(string workspaceId)
        {
            var association = _associationsManager.GetAssociation(workspaceId);
            if (association != null)
                return _youTrackService.GetYouTrackUsers(association.domain, association.permToken);
            
            return null;
        }
    }
}