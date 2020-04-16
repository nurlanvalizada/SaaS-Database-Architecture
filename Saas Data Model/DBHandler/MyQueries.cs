using System;

namespace Saas_Data_Model.DBHandler
{
    public static class MyQueries
    {
        public const string AllTenantTablesListQuery =
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME not " +
            "like '%sys%' and TABLE_NAME not like '%_Migration%' and TABLE_NAME not like '%MetaData%'";

        public static string GetForeignKeyQuery(string senderTable, string sentColumn)
        {
            return string.Format(@"SELECT  obj.name AS FK_NAME,
    tab1.name AS [table],
    col1.name AS [column],
    tab2.name AS [referenced_table],
    col2.name AS [referenced_column]
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj
    ON obj.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tab1
    ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch
    ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1
    ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
INNER JOIN sys.tables tab2
    ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2
    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id 
	and tab1.name='{0}' and col1.name='{1}'", senderTable, sentColumn);
        }

        public static string GetPrimaryKeyQuery(string senderTable)
        {
            return String.Format(@"SELECT i.name AS IndexName, OBJECT_NAME(ic.OBJECT_ID) AS TableName, 
       COL_NAME(ic.OBJECT_ID,ic.column_id) AS ColumnName
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic
ON i.OBJECT_ID = ic.OBJECT_ID
AND i.index_id = ic.index_id
WHERE i.is_primary_key = 1 and OBJECT_NAME(ic.OBJECT_ID)='{0}'", senderTable);
        }

        public static string GetForeignKeyQueryBetweenTablesQuery(string mainTable, string referencedTable)
        {
            return String.Format(@"SELECT  obj.name AS FK_NAME,
    tab1.name AS [table],
    col1.name AS [column],
    tab2.name AS [referenced_table],
    col2.name AS [referenced_column]
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj
    ON obj.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tab1
    ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch
    ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1
    ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
INNER JOIN sys.tables tab2
    ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2
    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id 
	and tab1.name='{0}' and tab2.name='{1}'", mainTable, referencedTable);
        }

        public static string ManyToManyTableCheckingQuery(string senderTable, string receivedTable)
        {
            return
                String.Format(
                    @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES where TABLE_NAME not in ('{0}','{1}')" +
                    " and TABLE_NAME not like '%sys%' and  TABLE_NAME not like '%_Migration%' and TABLE_NAME not like '%MetaData%'",
                    senderTable, receivedTable);
        }

        public static string OtherTableForeignKeysQuery(string table, string senderTable, string receivedTable, PrimaryKeyInfo senderprimaryKeyInfo, PrimaryKeyInfo receiverprimaryKeyInfo)
        {
            return String.Format(@"SELECT  obj.name AS FK_NAME,
    tab1.name AS [table],
    col1.name AS [column],
    tab2.name AS [referenced_table],
    col2.name AS [referenced_column]
FROM sys.foreign_key_columns fkc
INNER JOIN sys.objects obj
    ON obj.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tab1
    ON tab1.object_id = fkc.parent_object_id
INNER JOIN sys.schemas sch
    ON tab1.schema_id = sch.schema_id
INNER JOIN sys.columns col1
    ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id
INNER JOIN sys.tables tab2
    ON tab2.object_id = fkc.referenced_object_id
INNER JOIN sys.columns col2
    ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id 
	and tab1.name='{0}' and tab2.name in ('{1}','{2}') " +
                  "and col2.name in ('{3}','{4}')", table, senderTable, receivedTable, senderprimaryKeyInfo.ColumnName, receiverprimaryKeyInfo.ColumnName);
        }

        public static string GetColumnsListQuery(string tableName)
        {
            return string.Format(@"SELECT COLUMN_NAME,DATA_TYPE,IS_NULLABLE FROM 
INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = N'{0}'", tableName);
        }

        public static string GetTableGeneralSelectQuery(string tableName)
        {
            return string.Format(@"SELECT * FROM {0}", tableName);
        }
    }
}
