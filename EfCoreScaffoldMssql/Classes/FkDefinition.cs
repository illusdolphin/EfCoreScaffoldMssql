using System.Collections.Generic;

namespace EfCoreScaffoldMssql.Classes
{
    public class FkDefinitionSource
    {
        public string FkName { get; set;}
        public string FkSchema { get; set; }
        public string FkTable { get; set; }
        public string FkColumn { get; set; }
        public string PkName { get; set; }
        public string PkSchema { get; set; }
        public string PkTable { get; set; }
        public string PkColumn { get; set; }
        public string MatchOption { get; set; }
        public string UpdateRule { get; set; }
        public string DeleteRule { get; set; }
        public int PkOrdinalPosition { get; set; }
        public int FkOrdinalPosition { get; set; }

        public override string ToString()
        {
            return FkName;
        }
    }

    public class FkDefinition
    {
        public string FkName { get; set; }
        public string FkSchema { get; set; }
        public string FkTable { get; set; }
        public List<string> FkColumns { get; set; }
        public string PkName { get; set; }
        public string PkSchema { get; set; }
        public string PkTable { get; set; }
        public List<string> PkColumns { get; set; }
        public string MatchOption { get; set; }
        public string UpdateRule { get; set; }
        public string DeleteRule { get; set; }
        public bool IsCompositeKey => FkColumns.Count > 1;

        public override string ToString()
        {
            return FkName;
        }
    }
}