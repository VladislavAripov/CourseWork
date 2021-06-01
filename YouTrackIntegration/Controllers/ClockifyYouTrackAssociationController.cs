using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTrackIntegration.Services;
using YouTrackIntegration.Data;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClockifyYouTrackAssociationController : ControllerBase
    {
        private const string JsonPath = "Data/clockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackConnection> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackAssociationController> _logger;

        public ClockifyYouTrackAssociationController(ILogger<ClockifyYouTrackAssociationController> logger, ConnectionManger connectionManger)
        {
            _userAssociations = JsonSerializer.Deserialize<ClockifyYouTrackConnection[]>(_userAssociationsJson)
                ?.ToList();
            _logger = logger;
        }

        

        [HttpGet]
        public string GetAssociations()
        {
            return JsonSerializer.Serialize(_userAssociations);
        }
        
        [HttpPost]
        public string AddAssociation(ClockifyYouTrackConnection connection)
        {
            if (connection.IsValid())
            {
                _userAssociations.Add(connection);
                System.IO.File.WriteAllText(JsonPath, JsonSerializer.Serialize(_userAssociations));
                return _userAssociationsJson.ToString();
            }

            return "wrong data";
        }
    }
}