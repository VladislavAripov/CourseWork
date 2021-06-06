using System.Linq;
using YouTrackIntegration.Data;

namespace YouTrackIntegration.Services
{
    public class AssociationsManager
    {
        private readonly MyAppContext _context;
        
        public AssociationsManager(MyAppContext context)
        {
            _context = context;
        }

        public ClockifyYouTrackAssociation GetAssociation(string workspaceId)
        {
            return _context.Associations.FirstOrDefault(association => association.workspaceId == workspaceId);
        }
        
        public User GetUserFromAssociation(string workspaceId, string clockifyUserId)
        {
            var associationId = _context.Associations.FirstOrDefault(association => association.workspaceId == workspaceId)?.Id;
            if (associationId == null)
                return null;

            var userFromAssociation = _context.Users.FirstOrDefault(user => 
                user.ClockifyYouTrackAssociationId == associationId && 
                user.clockifyUserId == clockifyUserId);
            
            return userFromAssociation; // User or null.
        }

        public bool AddAssociation(string workspaceId, string domain, string permToken, string defaultIssueId)
        {
            if (IsAssociationExists(workspaceId))
                return false;

            var association = new ClockifyYouTrackAssociation(workspaceId, domain, permToken, defaultIssueId);
            _context.Associations.Add(association);
            _context.SaveChanges();
            
            return true;
        }

        private bool IsAssociationExists(string workspaceId)
        {
            var association = _context.Associations.FirstOrDefault(a => a.workspaceId == workspaceId);
            if (association != null)
                return false;

            return true;
        }

        public bool DeleteAssociation(string workspaceId)
        {
            var association = _context.Associations.FirstOrDefault(a => a.workspaceId == workspaceId);
            if (association == null)
                return false;

            _context.Associations.Remove(association);
            
            return true;
        }
    }
}