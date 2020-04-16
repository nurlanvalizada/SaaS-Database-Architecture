using System.Data.Entity;
using WebMatrix.WebData;

namespace SaaS_Database_Architecture.Models
{

    public class MyDatabaseInit : DropCreateDatabaseIfModelChanges<UsersContext>
    {
        protected override void Seed(UsersContext context)
        {
            SeedMembership();
        }

        private void SeedMembership()
        {
            WebSecurity.InitializeDatabaseConnection("Administration", "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }
    }

}