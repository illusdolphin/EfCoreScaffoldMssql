using System.Collections.Generic;

namespace EfCoreScaffoldMssql.Classes
{
    public class ObjectColumnsSettingModel
    {
        public string ObjectName { get; set; }
        public List<ColumnSettingModel> ColumnsList { get; set; }
    }

    public class ColumnSettingModel
    {
        public string Name { get; set; }
        public string NewName { get; set; }
    }
}
