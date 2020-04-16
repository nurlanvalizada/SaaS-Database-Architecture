namespace Saas_Data_Model
{
    public class ForeignKeyInfo
    {
        public string FkName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }
}