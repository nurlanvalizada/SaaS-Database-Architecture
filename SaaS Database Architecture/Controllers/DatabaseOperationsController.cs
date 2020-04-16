using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Saas_Data_Model;
using Saas_Data_Model.DBHandler;
using SaaS_Database_Architecture.Models;

namespace SaaS_Database_Architecture.Controllers
{
    public class DatabaseOperationsController : Controller
    {
        private string _connectionString;

        private readonly UsersContext _db = new UsersContext();

        public DatabaseOperationsController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Administration"].ConnectionString;
        }

        [HttpPost]
        public ActionResult AddNewField(NewField newField)
        {
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            MainDataModel.AddNewField(newField.tablename, newField.fieldname, newField.fieldtypes,newField.fieldsize,newField.defaultvalue,newField.allownull, _connectionString);
            return RedirectToAction("TableDescription", new {@tableName = newField.tablename});
        }

        [HttpPost]
        public ActionResult RemoveColumn(string columnName, string tableName)
        {
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            MainDataModel.RemoveColumn(columnName, tableName, _connectionString);
            return RedirectToAction("TableDescription", new {tableName});
        }

        [HttpGet]
        public ActionResult Tables()
        {
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            var tables = MainDataModel.TablesList(_connectionString);
            return View(tables);
        }

        [HttpGet]
        public ActionResult TableDescription(string tableName)
        {
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            ViewBag.TableName = tableName;
            List<ColumnInformation> columnsCustomSchema;
            var sharedColumns = MainDataModel.TableDescription(tableName, _connectionString, out columnsCustomSchema);
            ViewBag.CustomColumns = columnsCustomSchema;
            List<string> fieldTypesString = new List<string>(){"Sütun tipi","char",
"bit",
"datetime",
"decimal",
"float",
"int",
"nchar",
"nvarchar",
"ntext",
"timestamp"};
            var fieldTypes = fieldTypesString.Select(fieldType => new SelectListItem() { Text = fieldType, Value = fieldType }).ToList();
            ViewBag.FieldTypes = fieldTypes;
            return View(sharedColumns);
        }

        [HttpGet]
        public ActionResult TableData(string tableName)
        {
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            ViewBag.TableName = tableName;
            ViewBag.TableData = MainDataModel.TableData(tableName, _connectionString);
            return View();
        }

        [HttpGet]
        public ActionResult DatabaseSchemaCustomization()
        {
            //Session["ErrorMessage"] = null;
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            var entities = MainDataModel.DatabaseSchemaCustomizationGetRequest(_connectionString);
            return View(entities);
        }

        [HttpPost]
        public ActionResult DatabaseSchemaCustomization(string receivedTable, string senderTable, string sentColumn)
        {
            sentColumn = sentColumn.Trim();
            _connectionString = TenantOperations.GetTenantConnectionString(User.Identity.Name);
            Session["ErrorMessage"] = MainDataModel.DatabaseSchemaCustomizationPostRequest(receivedTable, senderTable,
                sentColumn, _connectionString);
            return RedirectToAction("DatabaseSchemaCustomization", "DatabaseOperations");
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
