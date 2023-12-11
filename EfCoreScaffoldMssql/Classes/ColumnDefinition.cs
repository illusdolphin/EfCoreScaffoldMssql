namespace EfCoreScaffoldMssql.Classes
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string SchemaName { get; set; }
        public string ObjectName { get; set; }
        public string ColumnTypeName { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsComputed { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string DefaultDefinition { get; set; }
        public string ComputedColumnSql { get; set; }
        public int ColumnId { get; set; }
        public string ExtendedPropertiesTypeName { get; set; }

        public override string ToString()
        {
            return $"{SchemaName}.{ObjectName}.{Name} {ExtendedPropertiesTypeName ?? ColumnTypeName}[{MaxLength}]";
        }
    }
}