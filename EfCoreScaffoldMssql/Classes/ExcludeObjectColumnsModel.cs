using System.Collections.Generic;

namespace EfCoreScaffoldMssql.Classes
{
    public class ExcludeObjectColumnsModel
    {
        public string ObjectName { get; set; }
        public List<string> ColumnNames { get; set; }
    }
}
