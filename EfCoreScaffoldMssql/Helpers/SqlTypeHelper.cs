using System;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class SqlTypeHelper
    {
        public static string GetCSharpType(this string sqlType)
        {
            switch (sqlType)
            {
                case "uniqueidentifier":
                    return nameof(Guid);

                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return nameof(DateTime);

                case "time":
                    return nameof(TimeSpan);

                case "tinyint":
                    return "byte";

                case "smallint":
                    return nameof(Int16);

                case "int":
                    return "int";

                case "real":
                    return nameof(Single);

                case "money":
                case "smallmoney":
                case "decimal":
                case "numeric":
                    return "decimal";

                case "float":
                    return "double";

                case "bit":
                    return "bool";

                case "bigint":
                    return nameof(Int64);

                case "binary":
                case "varbinary":
                    return "byte[]";

                case "varchar":
                case "nvarchar":
                case "nchar":
                case "char":
                case "text":
                case "ntext":
                    return "string";

                case "geometry":
                    return "Point";

                default:
                    return "object";
            }
        }
    }
}