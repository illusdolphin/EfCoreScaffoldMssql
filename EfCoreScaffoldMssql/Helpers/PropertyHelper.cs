using System.Text.RegularExpressions;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class PropertyHelper
    {
        public static string GetPropertyToDisplay (string originalProperty)
        {
            var regex = new Regex("-");
            var propertyToDisplay =  regex.Replace(originalProperty, "_");
            
            int intPropertyValue;
            var isIntValue = int.TryParse(propertyToDisplay, out intPropertyValue);
            if (isIntValue)
            {
                return string.Format("{0}{1}", "C", intPropertyValue);
            }

            return propertyToDisplay;
        }
    }
}
