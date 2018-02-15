using EfCoreScaffoldMssql.Helpers;

namespace EfCoreScaffoldMssql.Classes
{
    public class EntityDefinition
    {
        public string SchemaName { get; set; }
        public string EntityName { get; set; }

        public string EntityPluralizedName => StringHelper.Pluralize(EntityName);

        public override string ToString()
        {
            return $"{SchemaName}.{EntityName}";
        }
    }
}