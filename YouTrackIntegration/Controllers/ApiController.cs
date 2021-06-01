using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YouTrackIntegration.Services;
using YouTrackIntegration.Clockify;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("Api")]
    public class ApiController : ControllerBase
    {
        private ConnectionManger _connectionManger;
        private YouTrackService _youTrackService;
        
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger, ConnectionManger connectionManger, YouTrackService youTrackService)
        {
            _youTrackService = youTrackService;
            _connectionManger = connectionManger;
            
            _logger = logger;
        }
        
        
        [Route("Create")]
        [HttpPost]
        public void AddWorkItemToIssue(ClockifyApiModel request)
        {
            var connection = _connectionManger.GetConnection(request.workspaceId);
            if (connection != null)
                _youTrackService.CreateWorkItemInIssue(connection, request);
        }
        
        
        [Route("Update")]
        [HttpPost]
        public void UpdateTask(ClockifyApiModel request)
        {
            var connection = _connectionManger.GetConnection(request.workspaceId);
            if (connection != null)
                _youTrackService.UpdateWorkItemInIssue(connection, request);
        }


        [Route("Delete")]
        [HttpPost]
        public void DeleteTask(ClockifyApiModel request)
        {
            var connection = _connectionManger.GetConnection(request.workspaceId);
            if (connection != null)
                _youTrackService.DeleteWorkItemInIssue(connection, request);
        }

        [Route("GetUsers")]
        [HttpGet]
        public Task<String> GetUsers(string workspaceId)
        {
            var connection = _connectionManger.GetConnection(workspaceId);
            if (connection != null);
                return _youTrackService.GetYouTrackUsers(connection.domain, connection.permToken);
        }
    }
}