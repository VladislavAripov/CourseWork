using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTrackIntegration.Model;



namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClockifyYouTrackAssociationController : ControllerBase
    {
        private const string JsonPath = "Data/clockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackAssociation> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackAssociationController> _logger;

        public ClockifyYouTrackAssociationController(ILogger<ClockifyYouTrackAssociationController> logger)
        {
            _userAssociations = JsonSerializer.Deserialize<ClockifyYouTrackAssociation[]>(_userAssociationsJson)
                ?.ToList();
            _logger = logger;
        }

        

        [HttpGet]
        public string GetAssociations()
        {
            return JsonSerializer.Serialize(_userAssociations);
        }
        
        [HttpPost]
        public string AddAssociation(ClockifyYouTrackAssociation association)
        {
            if (association.IsValid())
            {
                _userAssociations.Add(association);
                System.IO.File.WriteAllText(JsonPath, JsonSerializer.Serialize(_userAssociations));
                return _userAssociationsJson.ToString();
            }

            return "wrong data";
        }
    }
}