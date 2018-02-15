
namespace EfCoreScaffoldMssql.Classes
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public string SchemaName { get; set; }
        public string ObjectName { get; set; }
        public string TypeName { get; set; }
        public int MaxLength { get; set; }
        public bool IsComputed { get; set; }
        public bool IsNullable { get; set; }
        public bool IsIdentity { get; set; }
        public string DefaultDefinition { get; set; }
        public int ColumnId { get; set; }

        public override string ToString()
        {
            return $"{SchemaName}.{ObjectName}.{Name} {TypeName}[{MaxLength}]";
        }
    }
}