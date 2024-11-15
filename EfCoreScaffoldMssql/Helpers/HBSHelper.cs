using EfCoreScaffoldMssql.Classes;
using HandlebarsDotNet;
using System;
using System.Linq;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class HBSHelper
    {
        public static void TableContainsAllColumns(EncodedTextWriter output, BlockHelperOptions options, Context context, Arguments arguments)
        {
            var stringArguments = arguments.Where(x => x is string).Select(x => x.ToString()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (stringArguments.Count > 0)
            {
                if (!(context.Value is EntityViewModel contextObject))
                {
                    Console.WriteLine($"#TableContainsAllColumns: Wring context: {context.Value.GetType().FullName}");
                    options.Inverse(output, context);
                    return;
                }
                
                var columnsList = contextObject.Columns;

                foreach (var stringArgument in stringArguments)
                {
                    var columnArguments = stringArgument.Split(':');
                    ColumnViewModel column;
                    if (columnArguments.Length > 1)
                    {
                        var isNullable = columnArguments[1] == "Nullable";
                        column = columnsList.FirstOrDefault(x => x.Name == columnArguments[0] && x.IsNullable == isNullable);
                    }
                    else
                    {
                        column = columnsList.FirstOrDefault(x => x.Name == columnArguments[0]);
                    }
                    if (column == null)
                    {
                        options.Inverse(output, context);
                        return;
                    }
                }
                options.Template(output, (object)context);
                return;
            }
            options.Inverse(output, context);
        }

        public static void Iif(EncodedTextWriter output, BlockHelperOptions options, Context context, Arguments arguments)
        {
            if (arguments.Length != 3)
            {
                output.Write("ifCond:Wrong number of arguments");
                return;
            }
            
            var left = arguments.At<string>(0);
            var op = arguments.At<string>(1);
            var right = arguments.At<string>(2);

            if (op != "==" && op != "!=")
            {
                output.Write("ifCond:Wrong operator");
                return;
            }

            if (op == "==")
            {
                if (left == right) options.Template(output, context);
                else options.Inverse(output, context);
                return;
            }

            if (left != right) options.Template(output, context);
            else options.Inverse(output, context);
        }
    }
}
