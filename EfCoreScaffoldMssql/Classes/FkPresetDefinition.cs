using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
