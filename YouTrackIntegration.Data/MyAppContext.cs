using Microsoft.EntityFrameworkCore;

namespace YouTrackIntegration.Data
{
    public class MyAppContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ClockifyYouTrackAssociation> Associations { get; set; }

        public MyAppContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            int baseAssociationId = 1;
            string baseAssociationWorkspaceId = "YourWorkspaceId";
            string baseAssociationDomain = "YourYouTrackDomain"; // Example: https://example.myjetbrains.com/youtrack
            string baseAssociationPermToken = "YourYouTrackPermToken";
            string baseAssociationDefaultIssueId = "YourDefaultIssueId"; // To save some unusual tasks.

            int baseUserId = 1;
            int clockifyYouTrackAssociationId = baseAssociationId;
            string baseUserClockifyUserId = "YourClockifyUserId";
            string baseUserYouTrackUserLogin = "YourYouTrackLogin";
            string baseUserDefaultIssueId = "DefaultIssueIdOfSpecificUser"; // To save some unusual tasks of specific user.
            
            
            // Adding records to db.
            var baseAssociationUser = new User { Id = baseUserId, ClockifyYouTrackAssociationId = clockifyYouTrackAssociationId,
                clockifyUserId = baseUserClockifyUserId, youTrackUserLogin = baseUserYouTrackUserLogin, 
                defaultIssueId = baseUserDefaultIssueId };
            var baseAssociation = new ClockifyYouTrackAssociation { Id = baseAssociationId,
                workspaceId = baseAssociationWorkspaceId, domain = baseAssociationDomain, 
                permToken = baseAssociationPermToken, defaultIssueId = baseAssociationDefaultIssueId };
            
            
            modelBuilder.Entity<User>().HasData( new User[] { baseAssociationUser });
            modelBuilder.Entity<ClockifyYouTrackAssociation>().HasData(new ClockifyYouTrackAssociation[] { baseAssociation });
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}