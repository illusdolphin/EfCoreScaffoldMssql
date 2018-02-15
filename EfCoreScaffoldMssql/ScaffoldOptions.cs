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

        public List<string> IgnoreViews { get; set; }
    }
}