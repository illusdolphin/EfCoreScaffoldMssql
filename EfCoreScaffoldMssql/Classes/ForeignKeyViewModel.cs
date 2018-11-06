namespace EfCoreScaffoldMssql.Classes
{
    public class ForeignKeyViewModel: FkDefinition
    {
        public string PropertyName { get; set; }

        public string InversePropertyName { get; set; }
    }
}