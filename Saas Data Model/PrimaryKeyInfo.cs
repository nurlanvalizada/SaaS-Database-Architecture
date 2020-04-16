namespace Saas_Data_Model
{
    public class PrimaryKeyInfo
    {
        public string PkName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
    }

    public class PrimaryKeyInfoForCompare
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
    }
}