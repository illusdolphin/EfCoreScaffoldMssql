using EfCoreScaffoldMssql.Classes;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class PropertyHelper
    {
        public static string GetColumnNameToDisplay(string originalProperty, string objectName = null, List<ObjectColumnsSettingModel> objectsColumnsSettings = null)
        {
            var nameToDisplay = "";

            if (objectsColumnsSettings != null && !string.IsNullOrEmpty(objectName))
            {
                var objectColumnsSetting = objectsColumnsSettings.Find(x => x.ObjectName == objectName);
                if (objectColumnsSetting != null)
                {
                    nameToDisplay = objectColumnsSetting.ColumnsList.Find(x => x.Name == originalProperty && x.NewName != null)?.NewName;
                    if (!string.IsNullOrEmpty(nameToDisplay))
                    {
                        return nameToDisplay;
                    }
                }
            }

            var regex = new Regex("-");
            nameToDisplay =  regex.Replace(originalProperty, "_");
            
            int intPropertyValue;
            var isIntValue = int.TryParse(nameToDisplay, out intPropertyValue);
            if (isIntValue)
            {
                return string.Format("{0}{1}", "C", intPropertyValue);
            }

            return nameToDisplay;
        }
    }
}
