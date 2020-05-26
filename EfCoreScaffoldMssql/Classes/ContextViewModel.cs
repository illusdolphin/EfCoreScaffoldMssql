using System.Collections.Generic;

namespace EfCoreScaffoldMssql.Classes
{
    public class ContextViewModel
    {
        public string ContextName { get; set; }

        public string Namespace { get; set; }

        public List<EntityViewModel> Entities { get; set; }

        public List<StoredObjectDefinition> StoredProcedures { get; set; }

        public List<StoredObjectDefinition> TableValuedFunctions { get; set; }

        public ContextViewModel()
        {
            Entities = new List<EntityViewModel>();
            StoredProcedures = new List<StoredObjectDefinition>();
            TableValuedFunctions = new List<StoredObjectDefinition>();
        }
    }
}