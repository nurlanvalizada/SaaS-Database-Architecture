using System;
using System.Linq;

namespace SaaS_Database_Architecture.Models
{
    public class TenantOperations
    {
        static string _connectionString;
        
        public static string GetTenantConnectionString(string username)
        {
            var db = new UsersContext();
            string tenantDatabaseName = null;
            var userprofile = db.UserProfiles.FirstOrDefault(p => p.UserName == username);
            if (userprofile != null && userprofile.UserTenantProfile != null)
            {
                var profile = db.TenantProfiles.FirstOrDefault(p => p.TenantId == userprofile.UserTenantProfile.TenantId);
                if (profile != null) tenantDatabaseName = profile.TenantDatabaseName;
            }
            _connectionString = String.Format("Data Source=127.0.0.1;Initial Catalog={0};Integrated Security=True", tenantDatabaseName);
            return _connectionString;
        }
    }
}