using EfCoreScaffoldMssql.Classes;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class HBSHelper
    {
        public static void TableContainsAllColumns(TextWriter output, HelperOptions options, dynamic context, params object[] arguments)
        {
            var stringArguments = arguments.Where(x => x is string).Select(x => x.ToString()).Where(x => !string.IsNullOrEmpty(x)).ToList<string>();
            if (context is object && stringArguments != null && stringArguments.Count > 0)
            {
                var contextObject = (object)context;
                var columnsList = new List<ColumnViewModel>();
                var columns = contextObject.GetType().GetProperty("Columns")?.GetValue(contextObject, null);
                if (columns != null && columns is IList<ColumnViewModel>)
                {
                    columnsList = ((IList<ColumnViewModel>)columns).ToList();
                }
                else
                {
                    Console.WriteLine($"#TableContainsAllColumns: Can not find columns for type {contextObject.GetType().GetProperty("EntityName")?.GetValue(contextObject, null)}");
                    options.Inverse(output, (object)context);
                    return;
                }

                foreach (var stringArgument in stringArguments)
                {
                    var columnArguments = stringArgument.Split(':');
                    ColumnViewModel column = null;
                    if (columnArguments.Length > 1)
                    {
                        var isNullable = columnArguments[1] == "Nullable" ? true : false;
                        column = columnsList.FirstOrDefault(x => x.Name == columnArguments[0] && x.IsNullable == isNullable);
                    }
                    else
                    {
                        column = columnsList.FirstOrDefault(x => x.Name == columnArguments[0]);
                    }
                    if (column == null)
                    {
                        options.Inverse(output, (object)context);
                        return;
                    }
                }
                options.Template(output, (object)context);
                return;
            }
            options.Inverse(output, (object)context);
        }
    }
}
