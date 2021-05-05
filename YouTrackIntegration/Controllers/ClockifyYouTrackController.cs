using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using YouTrackIntegration.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace YouTrackIntegration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClockifyYouTrackController : ControllerBase
    {
        private const string JsonPath = "Data/clockifyYouTrackAssociations.json";
        private string _userAssociationsJson = System.IO.File.ReadAllText(JsonPath);
        private readonly List<ClockifyYouTrackAssociation> _userAssociations;
        
        private readonly ILogger<ClockifyYouTrackController> _logger;

        public ClockifyYouTrackController(ILogger<ClockifyYouTrackController> logger)
        {
            _userAssociations = JsonSerializer.Deserialize<ClockifyYouTrackAssociation[]>(_userAssociationsJson)
                ?.ToList();
            _logger = logger;
        }

        
        
        [HttpPost]
        public string AddSpentTimeToTask(ClockifyApiModel request)
        {
            var association = _userAssociations.Find(a => a.clockifyId == request.userId);
            if (association != null)
            {
                var youTrack = FindYouTrack(association, request.description);
                if (youTrack != null)
                {
                    var start = request.timeInterval.start;
                    var end = request.timeInterval.end;
            
                    var spentTime = end - start;
            
                    var spentHours = spentTime.Hours;
                    var spentMinutes = spentTime.Minutes;
                    spentMinutes += (spentTime.Seconds > 30) ? 1 : 0;
                
                    return $"spent hours: {spentHours}; spent minutes: {spentMinutes}";
                }
            }

            return "Wrong";
        }


        private YouTrack FindYouTrack(ClockifyYouTrackAssociation association, string description)
        {
            foreach (var youTrack in association.youTracks)
                if (youTrack.taskKey == description.Substring(0, youTrack.taskKey.Length))
                    return youTrack;

            return null;
        }
    }
}