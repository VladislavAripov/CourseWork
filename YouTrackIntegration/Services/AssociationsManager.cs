using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using YouTrackIntegration.Data;

namespace YouTrackIntegration.Services
{
    public class AssociationsManager
    {
        private const string JsonPath = "Data/ClockifyYouTrackAssociations.json";
        private List<ClockifyYouTrackAssociation> _associations;
        
        public AssociationsManager()
        {
            _associations = JsonSerializer.Deserialize<ClockifyYouTrackAssociation[]>(File.ReadAllText(JsonPath))
                ?.ToList();
        }

        public ClockifyYouTrackAssociation GetAssociation(string workspaceId)
        {
            return _associations.FirstOrDefault(association => association.workspaceId == workspaceId);
        }

        public bool AddAssociation(string userId, string workspaceId, string domain, string permToken)
        {
            if (_associations.FirstOrDefault(a => a.workspaceId == workspaceId) != null)
                return false;
            
            var association = new ClockifyYouTrackAssociation(userId, workspaceId, domain, permToken);
            _associations.Add(association);
            File.WriteAllText(JsonPath, JsonSerializer.Serialize(_associations));
            
            return true;
        }

        public bool DeleteAssociation(string workspaceId)
        {
            var isSuccess = _associations.Remove(_associations
                .FirstOrDefault(association => association.workspaceId == workspaceId));
            if (isSuccess)
                File.WriteAllText(JsonPath, JsonSerializer.Serialize(_associations));    
            
            return isSuccess;
        }
    }
}