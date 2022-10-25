using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EfCoreScaffoldMssql
{
    public class ScaffoldOptions
    {
        public ScaffoldOptions()
        {
            Schemas = new List<string>();
            IgnoreTables = new List<string>();
            IgnoreViews = new List<string>();
            IgnoreStoredProcedure = new List<string>();
            IgnoreTableValuedFunctions = new List<string>();
            AllowedTables = new List<string>();
            IncludeTables = new List<string>();
        }

        public string ConnectionString { get; set; }

        public string TemplatesDirectory { get; set; }

        public string Directory { get; set; }

        public string ModelsPath { get; set; }

        public string Namespace { get; set; }

        public string ContextName { get; set; }

        public string ForeignPropertyRegex { get; set; }

        public bool IsVerbose { get; set; }

        public List<string> Schemas { get; set; }

        public List<string> IgnoreTables { get; set; }

        public List<string> AllowedTables { get; set; }

        public List<string> IncludeTables { get; set; }

        public List<string> IgnoreViews { get; set; }

        public bool GenerateStoredProcedures { get; set; }

        public List<string> IgnoreStoredProcedure { get; set; }

        public bool GenerateTableValuedFunctions { get; set; }

        public List<string> IgnoreTableValuedFunctions { get; set; }

        public string ExtendedPropertyTypeName { get; set; }

        public bool CleanUp { get; set; }
        public string CustomSettingsJsonPath { get; set; }

        public bool AllowManyToMany { get; set; }
        public bool NullableReferenceTypes { get; set; }

        public List<string> IgnoreObjectsForManyToMany { get; set; }
    }
}