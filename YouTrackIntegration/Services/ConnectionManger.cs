using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using YouTrackIntegration.Data;

namespace YouTrackIntegration.Services
{
    public class ConnectionManger
    {
        private const string JsonPath = "Data/ClockifyYouTrackAssociations.json";
        private List<ClockifyYouTrackConnection> _connections;
        
        public ConnectionManger()
        {
            _connections = JsonSerializer.Deserialize<ClockifyYouTrackConnection[]>(System.IO.File.ReadAllText(JsonPath))
                ?.ToList();
        }

        public ClockifyYouTrackConnection GetConnection(string workspaceId)
        {
            return _connections.FirstOrDefault(connection => connection.workspaceId == workspaceId);
        }

        public bool AddConnection(string userId, string workspaceId, string domain, string permToken)
        {
            if (_connections.FirstOrDefault(c => c.workspaceId == workspaceId) != null)
                return false;
            
            var connection = new ClockifyYouTrackConnection(userId, workspaceId, domain, permToken);
            _connections.Add(connection);
            File.WriteAllText(JsonPath, JsonSerializer.Serialize(_connections));
            
            return true;
        }

        public bool DeleteConnection(string workspaceId)
        {
            var isSuccess =
                _connections.Remove(_connections.FirstOrDefault(connection => connection.workspaceId == workspaceId));
            if (isSuccess)
                File.WriteAllText(JsonPath, JsonSerializer.Serialize(_connections));    
            
            return isSuccess;
        }
    }
}