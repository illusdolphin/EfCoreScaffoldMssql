﻿using System.Collections.Generic;
using System.Linq;

namespace EfCoreScaffoldMssql.Classes
{
    public class EntityViewModel: EntityDefinition
    {
        public EntityViewModel()
        {
            Columns = new List<ColumnViewModel>();
            Keys = new List<KeyColumnViewModel>();
            ForeignKeys = new List<ForeignKeyViewModel>();
            InverseKeys = new List<ForeignKeyViewModel>();
            Dependencies = new List<Dependency>();
        }

        public string Namespace { get; set; }
        public string EntityTableName { get; set; }
        public string EntityPluralizedName { get; set; }

        public bool IsVirtual { get; set; }

        public bool HasForeignKeys => ForeignKeys.Any();

        public bool HasKey => Keys.Any();
        public bool HasTriggers => Triggers.Any();
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

        public List<KeyColumnViewModel> Keys { get; set; }

        public List<ForeignKeyViewModel> ForeignKeys { get; set; }

        public List<ForeignKeyViewModel> InverseKeys { get; set; }

        public List<TriggerViewModel> Triggers { get; set; }

        public List<Dependency> Dependencies { get; set; }
        public bool IsDefaultSchema { get; set; }

        public bool IsManyToMany { get; set; }
        public string ManyToManyLeftTable { get; set; }
        public string ManyToManyRightTable { get; set; }
        public string ManyToManyLeftInversePropertyName { get; set; }
        public string ManyToManyRightInversePropertyName { get; set; }
    }

    public class Dependency
    {
        public string Name { get; set; }
    }
}