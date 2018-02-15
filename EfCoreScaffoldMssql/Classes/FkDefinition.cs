namespace EfCoreScaffoldMssql.Classes
{
    public class FkDefinition
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

        public override string ToString()
        {
            return FkName;
        }
    }
}