using System.Collections.Generic;

namespace EfCoreScaffoldMssql.Classes
{
    public class ContextViewModel
    {
        public string ContextName { get; set; }

        public string Namespace { get; set; }

        public List<EntityViewModel> Entities { get; set; }

        public ContextViewModel()
        {
            Entities = new List<EntityViewModel>();
        }
    }
}