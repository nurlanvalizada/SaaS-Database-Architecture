using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Saas_Data_Model.DBHandler
{
    public partial class MainDataModel
    {
        public static Dictionary<string, List<ColumnInformation>> DatabaseSchemaCustomizationGetRequest(string connectionString)
        {
            var entities = new Dictionary<string, List<ColumnInformation>>();
            var tableNames = new List<string>();
            var columns = new List<ColumnInformation>();
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmd = new SqlCommand(MyQueries.AllTenantTablesListQuery, con);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    tableNames.Add(dr[0].ToString());
                }
                dr.Close();
                foreach (var tableName in tableNames)
                {
                    columns.Clear();
                    SqlCommand cmdTable = new SqlCommand
                    {
                        Connection = con,
                        CommandText = MyQueries.GetTableGeneralSelectQuery(tableName)
                    };
                    var da = new SqlDataAdapter(cmdTable);
                    var dt = new DataTable(tableName);
                    da.Fill(dt);
                    columns.AddRange(from DataColumn column in dt.Columns select new ColumnInformation { Name = column.ColumnName, DataTypeName = column.DataType.Name, AllowDbNull = column.AllowDBNull.ToString() });
                    entities.Add(tableName, new List<ColumnInformation>(columns));
                }
            }
            return entities;
        }

        public static string DatabaseSchemaCustomizationPostRequest(string receivedTable, string senderTable, string sentColumn, string connectionString)
        {
            string errorMessage;
            ForeignKeyInfo oneToOneForeignkey;
            ForeignKeyInfo manyToOneForeignkey;
            string manyToManyTableName;
            var manyToOneCon = false;
            var oneToOneCon = false;
            var oneToManyCon = false;
            var manyToManyCon = CheckManyToMany(receivedTable, senderTable, connectionString, out manyToManyTableName);
            var result = CheckOneToOne(receivedTable, senderTable, connectionString, out oneToOneForeignkey);
            switch (result)
            {
                case "onetoone":
                    oneToOneCon = true;
                    break;
                case "onetomany":
                    oneToManyCon = true;
                    break;
            }
            var answer = CheckManyToOne(receivedTable, senderTable, connectionString, out manyToOneForeignkey);
            switch (answer)
            {
                case "onetoone":
                    oneToOneCon = true;
                    break;
                case "manytoone":
                    manyToOneCon = true;
                    break;
            }
            var noConnection = !(manyToManyCon || oneToOneCon || oneToManyCon || manyToOneCon);
            oneToOneForeignkey = oneToOneForeignkey.ColumnName == null ? manyToOneForeignkey : oneToOneForeignkey;
            manyToOneForeignkey = manyToOneForeignkey ?? oneToOneForeignkey;
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                PrimaryKeyInfo pkInfo = GetPrimaryKey(con, senderTable);
                if (pkInfo != null && pkInfo.ColumnName == sentColumn)
                {
                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                  receivedTable + " cədvəlinə köçürülə bilməz.Çünki bu sütun " + senderTable + " cədvəlinin əsas açarıdır..";
                    return errorMessage;
                }
                ForeignKeyInfo fkInfo = GetForeignKey(con, senderTable, sentColumn);
                if (fkInfo != null)
                {
                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                  receivedTable + " cədvəlinə köçürülə bilməz.Çünki bu sütun " + senderTable + " cədvəlinin xarici açarıdır və bu xarici açarın əlaqələndiyi sütun " + fkInfo.ReferencedTable + " cədvəlinin " + fkInfo.ReferencedColumn + " sütunudur..";
                    return errorMessage;
                }
                int columnCount = GetColumnList(con, senderTable).Count;
                if (columnCount < 2)
                {
                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                 receivedTable + " cədvəlinə köçürülə bilməz.Çünki bu sütun bu cədvəldə olan yeganə sütundur..";
                    return errorMessage;
                }
                List<string> receivedTableColumnList = GetColumnList(con, receivedTable);
                if (receivedTableColumnList.Contains(sentColumn))
                {

                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                 receivedTable + " cədvəlinə köçürülə bilməz.Çünki " + receivedTable + " cədvəlində artıq " + sentColumn + " adlı sütun vardır.Zəhmət olmasa sütun adını dəyişib əməliyyatı yenidən təkrar edin..";
                    return errorMessage;
                }

                if (manyToManyCon)
                {
                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                   receivedTable + " cədvəlinə köçürülə bilməz.Çünki " + senderTable + " və " + receivedTable + " cədvəlləri arasında " + manyToManyTableName + " cədvəli vasitəsi ilə ~çoxun-çoxa~ əlaqəsi vardır..";
                    return errorMessage;
                }
                if (manyToOneCon)
                {
                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                   receivedTable + " cədvəlinə köçürülə bilməz.Çünki " + senderTable + " və " + receivedTable + " cədvəlləri arasında ~çoxun-birə~ əlaqəsi vardır..";
                    return errorMessage;
                }
                if (noConnection)
                {
                    errorMessage = "" + sentColumn + " sütunu " + senderTable + " cədvəlindən " +
                                   receivedTable + " cədvəlinə köçürülə bilməz.Çünki " + senderTable + " və " + receivedTable + " cədvəlləri arasında ~birbaşa olaraq~ əlaqə yoxdur..";
                    return errorMessage;
                }

                errorMessage = MoveColumnWithData(con, receivedTable, senderTable, sentColumn, oneToOneCon, oneToManyCon, oneToOneForeignkey, manyToOneForeignkey);
            }
            return errorMessage;
        }

        private static string MoveColumnWithData(SqlConnection con, string receivedTable, string senderTable, string sentColumn, bool oneToOneCon, bool oneToManyCon, ForeignKeyInfo oneToOneforeignkey, ForeignKeyInfo foreignkey)
        {
            string errorMessage = null;
            SqlTransaction transaction = con.BeginTransaction();
            try
            {
                var cmdAddedColumn = new SqlCommand(@"ALTER TABLE " + receivedTable + " ADD " + sentColumn + " nvarchar(max)", con, transaction);
                cmdAddedColumn.ExecuteNonQuery();

                if (oneToOneCon)
                {
                    errorMessage = "" + sentColumn + " sütunu verilənlərlə birlikdə " + senderTable + " cədvəlindən " + receivedTable + " cədvəlinə köçürüldü.Beləki," +
                        senderTable + " və " + receivedTable + " cədvəlləri arasında ~birin-birə~ əlaqəsi vardır..";
                    var cmdInsertData = new SqlCommand(@"update " + receivedTable + " set " + sentColumn + "=(Select " + sentColumn + " from " + senderTable + " sd where sd." + oneToOneforeignkey.ColumnName + " =" + receivedTable + "." + oneToOneforeignkey.ReferencedColumn + ")", con, transaction);
                    cmdInsertData.ExecuteNonQuery();
                }
                else if (oneToManyCon)
                {
                    errorMessage = "" + sentColumn + " sütunu verilənlərilə birlikdə " + senderTable + " cədvəlindən " + receivedTable + " cədvəlinə köçürüldü.Beləki," +
                        senderTable + " və " + receivedTable + " cədvəlləri arasında ~birin-çoxa~ əlaqəsi vardır..";
                    var cmdInsertData = new SqlCommand(@"update " + receivedTable + " set " + sentColumn + "=(Select " + sentColumn + " from " + senderTable + " sd where sd." + foreignkey.ReferencedColumn + " =" + receivedTable + "." + foreignkey.ColumnName + ")", con, transaction);
                    cmdInsertData.ExecuteNonQuery();
                }

                var cmdDroppedColumn = new SqlCommand(@"ALTER TABLE " + senderTable + " DROP COLUMN " + sentColumn + "", con, transaction);
                cmdDroppedColumn.ExecuteNonQuery();

                var cmdMigrationHistory = new SqlCommand(@"Insert into _MigrationHistory values('" + senderTable + "','" + receivedTable + "','" + sentColumn + "','" + DateTime.Now + "')", con, transaction);
                cmdMigrationHistory.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            return errorMessage;
        }

        private static List<string> GetColumnList(SqlConnection con, string tableName)
        {
            var colList = new List<string>();
         
            var cmd = new SqlCommand
            {
                Connection = con,
                CommandText = MyQueries.GetColumnsListQuery(tableName)
            };
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                colList.Add(dr[0].ToString());
            }
            dr.Close();
            return colList;
        }

        private static PrimaryKeyInfo GetPrimaryKey(SqlConnection con, string tableName)
        {
            var cmdPrimaryKeys = new SqlCommand
            {
                CommandText = MyQueries.GetPrimaryKeyQuery(tableName),
                CommandType = CommandType.Text,
                Connection = con
            };

            var drPrimaryKeys = cmdPrimaryKeys.ExecuteReader();
            PrimaryKeyInfo primaryKey = new PrimaryKeyInfo();
            if (drPrimaryKeys.Read())
            {
                primaryKey = new PrimaryKeyInfo
                {
                    PkName = drPrimaryKeys[0].ToString(),
                    TableName = drPrimaryKeys[1].ToString(),
                    ColumnName = drPrimaryKeys[2].ToString()
                };
            }
            drPrimaryKeys.Close();
            return primaryKey;
        }

        private static ForeignKeyInfo GetForeignKey(SqlConnection con, string senderTable, string sentColumn)
        {
            var cmdForeignKeyReferences = new SqlCommand
            {
                CommandText = MyQueries.GetForeignKeyQuery(senderTable, sentColumn),
                CommandType = CommandType.Text,
                Connection = con
            };

            var drForeignKeyReferences = cmdForeignKeyReferences.ExecuteReader();
            ForeignKeyInfo foreignkey = null;
            if (drForeignKeyReferences.Read())
            {
                foreignkey = new ForeignKeyInfo
                {
                    FkName = drForeignKeyReferences[0].ToString(),
                    TableName = drForeignKeyReferences[1].ToString(),
                    ColumnName = drForeignKeyReferences[2].ToString(),
                    ReferencedTable = drForeignKeyReferences[3].ToString(),
                    ReferencedColumn = drForeignKeyReferences[4].ToString()
                };
            }
            drForeignKeyReferences.Close();
            return foreignkey;
        }

        private static bool CheckManyToMany(string receivedTable, string senderTable, string connectionString, out string matchedTable)
        {
            var tables = new List<string>();

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();

                PrimaryKeyInfo receiverprimaryKeyInfo = GetPrimaryKey(con, receivedTable);
                PrimaryKeyInfo senderprimaryKeyInfo = GetPrimaryKey(con, senderTable);

                SqlCommand cmd = new SqlCommand
                {
                    Connection = con,
                    CommandText = MyQueries.ManyToManyTableCheckingQuery(senderTable, receivedTable)
                };
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    tables.Add(dr[0].ToString());
                }
                dr.Close();

                var otherTableFKs = new Dictionary<string, List<ForeignKeyInfo>>();
                foreach (var table in tables)
                {
                    var otherTableForeignKeysCommand = new SqlCommand
                    {
                        Connection = con,
                        CommandText =
                            MyQueries.OtherTableForeignKeysQuery(table, senderTable, receivedTable, senderprimaryKeyInfo,
                                receiverprimaryKeyInfo)
                    };
                    var drotherTableForeignKeysCommand = otherTableForeignKeysCommand.ExecuteReader();
                    var otherTableForeignKeyInfos = new List<ForeignKeyInfo>();
                    while (drotherTableForeignKeysCommand.Read())
                    {
                        var fkInfo = new ForeignKeyInfo
                        {
                            FkName = drotherTableForeignKeysCommand[0].ToString(),
                            TableName = drotherTableForeignKeysCommand[1].ToString(),
                            ColumnName = drotherTableForeignKeysCommand[2].ToString(),
                            ReferencedTable = drotherTableForeignKeysCommand[3].ToString(),
                            ReferencedColumn = drotherTableForeignKeysCommand[4].ToString()
                        };
                        otherTableForeignKeyInfos.Add(fkInfo);
                    }
                    drotherTableForeignKeysCommand.Close();
                    otherTableFKs.Add(table, otherTableForeignKeyInfos);

                }

                var primaryKeyInfoForCompareSenderTable = new PrimaryKeyInfoForCompare
                {
                    ColumnName = senderprimaryKeyInfo.ColumnName,
                    TableName = senderprimaryKeyInfo.TableName
                };

                var primaryKeyInfoForCompareReceiverTable = new PrimaryKeyInfoForCompare
                {
                    ColumnName = receiverprimaryKeyInfo.ColumnName,
                    TableName = receiverprimaryKeyInfo.TableName
                };


                var primaryKeyInfosForCompare = new Dictionary<string, List<PrimaryKeyInfoForCompare>>();
                foreach (var otherTableFk in otherTableFKs)
                {
                    var listt = otherTableFk.Value.Select(foreignKey => new PrimaryKeyInfoForCompare
                    {
                        ColumnName = foreignKey.ReferencedColumn,
                        TableName = foreignKey.ReferencedTable
                    }).ToList();
                    primaryKeyInfosForCompare.Add(otherTableFk.Key, listt);
                }

                foreach (var primaryKeyInfoForCompare in from primaryKeyInfoForCompare in primaryKeyInfosForCompare
                                                         let infosList = primaryKeyInfoForCompare.Value.ToList()
                                                         where
                                                             infosList.Any(p => p.ColumnName == primaryKeyInfoForCompareSenderTable.ColumnName &&
                                                             p.TableName == primaryKeyInfoForCompareSenderTable.TableName)
                                                         where
                                                             infosList.Any(s => s.ColumnName == primaryKeyInfoForCompareReceiverTable.ColumnName &&
                                                             s.TableName == primaryKeyInfoForCompareReceiverTable.TableName)
                                                         select primaryKeyInfoForCompare)
                {
                    matchedTable = primaryKeyInfoForCompare.Key;
                    return true;
                }

                matchedTable = null;
                return false;
            }
        }

        private static string CheckManyToOne(string receivedTable, string senderTable, string connectionString, out ForeignKeyInfo foreignKey)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmdForeignKeyReferences = new SqlCommand
                {
                    CommandText = MyQueries.GetForeignKeyQueryBetweenTablesQuery(senderTable, receivedTable),
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var drForeignKeyReferences = cmdForeignKeyReferences.ExecuteReader();
                ForeignKeyInfo foreignkey = null;
                if (drForeignKeyReferences.Read())
                {
                    foreignkey = new ForeignKeyInfo
                    {
                        FkName = drForeignKeyReferences[0].ToString(),
                        TableName = drForeignKeyReferences[1].ToString(),
                        ColumnName = drForeignKeyReferences[2].ToString(),
                        ReferencedTable = drForeignKeyReferences[3].ToString(),
                        ReferencedColumn = drForeignKeyReferences[4].ToString()
                    };
                }
                drForeignKeyReferences.Close();

                var primaryKey = GetPrimaryKey(con, senderTable);
                string result;
                if (foreignkey != null)
                {
                    result = "manytoone";
                    if (foreignkey.ColumnName == primaryKey.ColumnName)
                    {
                        result = (foreignkey.ColumnName == primaryKey.ColumnName) ? "onetoone" : "manytoone";
                    }
                }
                else
                {
                    result = null;
                }

                foreignKey = foreignkey;
                return result;
            }
        }

        private static string CheckOneToOne(string receivedTable, string senderTable, string connectionString, out ForeignKeyInfo oneToOneforeignkey)
        {
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                var cmdOneToOneRelationship = new SqlCommand
                {
                    CommandText = MyQueries.GetForeignKeyQueryBetweenTablesQuery(receivedTable, senderTable),
                    CommandType = CommandType.Text,
                    Connection = con
                };

                var primaryKey = GetPrimaryKey(con, receivedTable);
                var drOneToOneRelationship = cmdOneToOneRelationship.ExecuteReader();
                var oneToOneForeignKey = new ForeignKeyInfo();
                if (drOneToOneRelationship.Read())
                {
                    oneToOneForeignKey.FkName = drOneToOneRelationship[0].ToString();
                    oneToOneForeignKey.TableName = drOneToOneRelationship[1].ToString();
                    oneToOneForeignKey.ColumnName = drOneToOneRelationship[2].ToString();
                    oneToOneForeignKey.ReferencedTable = drOneToOneRelationship[3].ToString();
                    oneToOneForeignKey.ReferencedColumn = drOneToOneRelationship[4].ToString();
                }
                drOneToOneRelationship.Close();

                string result;

                if (oneToOneForeignKey.ColumnName != null)
                {
                    result = (primaryKey.ColumnName == oneToOneForeignKey.ColumnName) ? "onetoone" : "onetomany";
                }
                else
                {
                    result = "";
                }
                oneToOneforeignkey = oneToOneForeignKey;
                return result;
            }
        }

       
    }
}
