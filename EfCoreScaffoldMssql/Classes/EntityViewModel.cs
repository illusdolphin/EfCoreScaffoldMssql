using System.Collections.Generic;
using System.Linq;

namespace EfCoreScaffoldMssql.Classes
{
    public class EntityViewModel: EntityDefinition
    {
        public EntityViewModel()
        {
            Columns = new List<ColumnViewModel>();
            Keys = new List<KeyColumnDefinition>();
            ForeignKeys = new List<ForeignKeyViewModel>();
            InverseKeys = new List<ForeignKeyViewModel>();
            Dependencies = new List<Dependency>();
        }

        public string Namespace { get; set; }

        public bool HasForeignKeys => ForeignKeys.Any();

        public bool HasKey => Keys.Any();
        public bool HasSimpleKey => Keys.Count == 1;
        public bool HasDefaultSimpleKey
        {
            get
            {
                if (HasSimpleKey)
                {
                    var keyName = Keys.Single().ColumnName.ToLower();
                    return keyName == "id" || keyName == EntityName.ToLower() + "id";
                }

                return false;
            }
        }

        public List<ColumnViewModel> Columns { get; set; }

        public List<ColumnViewModel> ColumnsEfPropertyOrder => Columns
            .OrderBy(x => x.IsKey ? 0 : 1)
            .ThenBy(x => x.KeyColumnNumber)
            .ThenBy(x => x.Name).ToList();

        public bool HasInverseSets => InverseKeys.Any();

        public List<KeyColumnDefinition> Keys { get; set; }

        public List<ForeignKeyViewModel> ForeignKeys { get; set; }

        public List<ForeignKeyViewModel> InverseKeys { get; set; }
        public List<Dependency> Dependencies { get; set; }
        public bool IsDefaultSchema { get; set; }
    }

    public class Dependency
    {
        public string Name { get; set; }
    }
}