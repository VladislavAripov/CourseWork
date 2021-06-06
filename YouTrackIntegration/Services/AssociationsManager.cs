using System;
using System.Linq;
using YouTrackIntegration.Data;
using Microsoft.EntityFrameworkCore;

namespace YouTrackIntegration.Services
{
    public class AssociationsManager
    {
        private MyAppContext _context;
        
        public AssociationsManager(MyAppContext context)
        {
            _context = context;
        }

        public ClockifyYouTrackAssociation GetAssociation(string workspaceId)
        {
            return _context.Associations.FirstOrDefault(association => association.workspaceId == workspaceId);
        }

        public bool AddAssociation(string userId, string workspaceId, string domain, string permToken)
        {
            if (IsAssociationExists(workspaceId))
                return false;

            var association = new ClockifyYouTrackAssociation(userId, workspaceId, domain, permToken);
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