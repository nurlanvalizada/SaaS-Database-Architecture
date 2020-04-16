using System.Configuration;
using Saas_Data_Model;
using Saas_Data_Model.DBHandler;

namespace SaaS_Database_Architecture.Models
{
    public class NewTenantProvisioning
    {
        public static void DoProvisioning(TenantProfile tenantprofile)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Administration"].ConnectionString;
            MainDataModel.TenantProvisioning(tenantprofile.TenantDatabaseName, tenantprofile.TenantDatabaseUsername, tenantprofile.TenantDatabasePassword, connectionString);
        }
    }
}