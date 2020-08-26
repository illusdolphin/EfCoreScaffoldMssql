namespace EfCoreScaffoldMssql.Classes
{
    public class FkPresetDefinition
    {
        public string ForeignKeyName { get; set; }
        public FkPropertyNameDefinition FkPropertyNames { get; set; }
    }

    public class FkPropertyNameDefinition
    {
        public string PropertyName { get; set; }
        public string InversePropertyName { get; set; }
    }
}
