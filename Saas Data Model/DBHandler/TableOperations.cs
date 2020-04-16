using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Saas_Data_Model.DBHandler
{
    public partial class MainDataModel
    {
        public static void AddNewField(string tablename, string fieldname, string fieldtype,string fieldsize,string defaultvalue,bool allownull,string connectionString)
        {
            using (var con = new SqlConnection(connectionString))
            {
                string alterQuery;
                string metaDataInsertQuery;
                var allowedSizeTypes = new List<string>() {"char","nchar","nvarchar"};
                if (string.IsNullOrEmpty(fieldsize)||!allowedSizeTypes.Contains(fieldtype))
                {
                    alterQuery = "ALTER TABLE " + tablename + " ADD " + fieldname + " " + fieldtype;
                    metaDataInsertQuery = "Insert into MetaData values('" + tablename + "','" + fieldname + "','" +fieldtype + "')";
                }
                else
                {
                    
                    alterQuery = "ALTER TABLE " + tablename + " ADD " + fieldname + " " + fieldtype+"("+fieldsize+")";
                    metaDataInsertQuery = "Insert into MetaData values('" + tablename + "','" + fieldname + "','" + fieldtype+"("+fieldsize+")')";
                }
                if (!allownull)
                {
                    alterQuery += " not null ";
                }
                if (!String.IsNullOrEmpty(defaultvalue))
                {
                    var defaultStrings = new List<string>() {"char","nchar","nvarchar","ntext"};
                    if (defaultStrings.Contains(fieldtype))
                    {
                        alterQuery += " default '"+defaultvalue+"'";
                    }
                    else
                    {
                        alterQuery += " default " + defaultvalue;
                    }
                   
                }
               
                con.Open();
                var cmd = new SqlCommand {Connection = con, CommandText = alterQuery};
                var cmdInsertToMetaDataTable = new SqlCommand {Connection = con, CommandText = metaDataInsertQuery};

                cmd.ExecuteNonQuery();

                cmdInsertToMetaDataTable.ExecuteNonQuery();
            }
        }

        public static void RemoveColumn(string columnName, string tableName, string connectionString)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                // SqlCommand cmd = new SqlCommand(@"declare @q varchar(1000); set @q = 'alter table '+ @tableName+' drop column '+ @columnName+'; exec (@q), con);", con);
                var cmd = new SqlCommand(@"alter table " + tableName + " drop column " + columnName + "", con);
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@columnName", columnName);

                //SqlCommand cmdDeleteFromMetaDataTable = new SqlCommand("Delete from MetaData where TableName='@tableName' and [FieldName]='@columnName'", con);
                var cmdDeleteFromMetaDataTable =
                    new SqlCommand(
                        "Delete from MetaData where TableName='" + tableName + "' and [FieldName]='" + columnName + "'",
                        con);
                cmdDeleteFromMetaDataTable.Parameters.AddWithValue("@tableName", tableName);
                cmdDeleteFromMetaDataTable.Parameters.AddWithValue("@columnName", columnName);


                cmd.ExecuteNonQuery();
                cmdDeleteFromMetaDataTable.ExecuteNonQuery();
            }
        }

        public static List<string> TablesList(string connectionString)
        {
            var tables = new List<string>();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand {Connection = con, CommandText = MyQueries.AllTenantTablesListQuery};
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    tables.Add(dr[0].ToString());
                }
                dr.Close();
                cmd.ExecuteNonQuery();
                return tables;
            }
        }

        public static List<ColumnInformation> TableDescription(string tableName, string connectionString,
            out List<ColumnInformation> columnsCustom)
        {
            using (var con = new SqlConnection(connectionString))
            {
                var columnsSharedSchema = new List<ColumnInformation>();
                var columnsCustomSchema = new List<ColumnInformation>();

                var cmdMain = new SqlCommand(@"SELECT COLUMN_NAME,DATA_TYPE,IS_NULLABLE FROM 
INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = N'" + tableName +
                                                    "'  and COLUMN_NAME  not in (select Fieldname from MetaData where TableName=N'" +
                                                    tableName + "')", con);
                cmdMain.Parameters.AddWithValue("@tableName", tableName);
                // cmdMain.CommandType = CommandType.StoredProcedure;
                // cmdMain.Parameters.AddWithValue("@tableName1", tableName);
                var da = new SqlDataAdapter(cmdMain);

                var cmdMetaData = new SqlCommand(@"SELECT COLUMN_NAME,DATA_TYPE,IS_NULLABLE FROM 
INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = N'" + tableName +
                                                        "'  and COLUMN_NAME  in (select Fieldname from MetaData where TableName=N'" +
                                                        tableName + "')", con);
                cmdMetaData.Parameters.AddWithValue("@tableName", tableName);
                //cmdMetaData.Parameters.AddWithValue("@tableName1", tableName);

                var daCustomSchema = new SqlDataAdapter(cmdMetaData);

                var dt = new DataTable(tableName);
                var dtCustomSchema = new DataTable(tableName);
                da.Fill(dt);
                daCustomSchema.Fill(dtCustomSchema);

                foreach (DataRow row in dtCustomSchema.Rows)
                {
                    columnsCustomSchema.Add(new ColumnInformation
                    {
                        Name = row[0].ToString(),
                        DataTypeName = row[1].ToString(),
                        AllowDbNull = row[2].ToString()
                    });
                }
                foreach (DataRow row in dt.Rows)
                {
                    columnsSharedSchema.Add(new ColumnInformation
                    {
                        Name = row[0].ToString(),
                        DataTypeName = row[1].ToString(),
                        AllowDbNull = row[2].ToString()
                    });
                }


                columnsCustom = columnsCustomSchema;
                return columnsSharedSchema;
            }
        }

        public static Dictionary<string, List<ColumnInformation>> DatabaseSchemaOverview(string connectionString)
        {
            var entities = new Dictionary<string, List<ColumnInformation>>();
            var tableNames = new List<string>();
            var columns = new List<ColumnInformation>();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd =
                    new SqlCommand(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME not like '%sys%' and TABLE_NAME not like '%_Migration%' and TABLE_NAME not like '%MetaData%'",
                        con);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    tableNames.Add(dr[0].ToString());
                }
                dr.Close();

                foreach (var tableName in tableNames)
                {
                    columns.Clear();

                    // SqlCommand cmdTable = new SqlCommand(@"declare @q varchar(1000); set @q = 'SELECT * FROM ' + @tableName; exec (@q)", con);
                    var cmdTable = new SqlCommand(@"SELECT * FROM " + tableName + "", con);
                    cmdTable.Parameters.AddWithValue("@tableName", tableName);
                    cmdTable.CommandType = CommandType.Text;
                    var da = new SqlDataAdapter(cmdTable);
                    var dt = new DataTable();
                    da.Fill(dt);


                    foreach (DataColumn column in dt.Columns)
                    {
                        columns.Add(new ColumnInformation
                        {
                            Name = column.ColumnName,
                            DataTypeName = column.DataType.Name,
                            AllowDbNull = column.AllowDBNull.ToString()
                        });
                    }
                    entities.Add(tableName, new List<ColumnInformation>(columns));
                }
            }
            return entities;
        }

        public static DataTable TableData(string tableName, string connectionString)
        {
            var dtColumns = new DataTable(tableName);
            var dtRows = new DataTable(tableName);
            using (var con = new SqlConnection(connectionString))
            {
                var cmdColumns =
                    new SqlCommand(
                        @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = N'" + tableName + "'",
                        con);
                cmdColumns.Parameters.AddWithValue("@tableName", tableName);
                var daColumns = new SqlDataAdapter(cmdColumns);

                // SqlCommand cmdRows = new SqlCommand(@"declare @q varchar(1000); set @q = 'SELECT * from '+ @tableName; exec (@q), con);", con);
                var cmdRows = new SqlCommand(@"SELECT * from " + tableName + " ", con);
                cmdRows.Parameters.AddWithValue("@tableName", tableName);
                var daRows = new SqlDataAdapter(cmdRows);

                daColumns.Fill(dtColumns);
                daRows.Fill(dtRows);
            }
            return dtRows;


        }
    }
}
