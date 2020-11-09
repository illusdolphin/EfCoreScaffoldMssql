using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfCoreScaffoldMssql.Classes
{
    public class EntityPluralizeNameDefinition
    {
        public string EntityName { get; set; }
        public string NewEntityPluralizedName { get; set; }
    }
}
