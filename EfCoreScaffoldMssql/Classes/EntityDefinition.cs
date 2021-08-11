using EfCoreScaffoldMssql.Helpers;

namespace EfCoreScaffoldMssql.Classes
{
    public class EntityDefinition
    {
        public string SchemaName { get; set; }
        public string EntityName { get; set; }
        public string Definition { get; set; }
        public bool IsViewEntity { get; set; }

        public override string ToString()
        {
            return $"{SchemaName}.{EntityName}";
        }
    }
}