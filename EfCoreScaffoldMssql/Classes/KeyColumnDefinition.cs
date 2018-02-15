namespace EfCoreScaffoldMssql.Classes
{
    public class KeyColumnDefinition
    {
        public string TableSchema { get; set; }
        public string TableName { get; set; }
        public string KeyName { get; set; }
        public string ColumnName { get; set; }

        public override string ToString()
        {
            return $"{KeyName}.{ColumnName}";
        }
    }
}