using System.Collections.Generic;
using System.Linq;
using EfCoreScaffoldMssql.Helpers;

namespace EfCoreScaffoldMssql.Classes
{
    public class StoredObjectDefinition
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string ResultTypeName => Name + "_Result";
        public bool IsFunction { get; set; }
        public string Definition { get; set; }

        public List<StoredObjectParameter> Parameters { get; set; }

        public List<StoredObjectSetColumn> Columns { get; set; }

        public override string ToString()
        {
            var parameters = Parameters.OrderBy(x => x.Order).Select(x => x.ParameterName);
            return $"{Schema}.{Name} {string.Join(", ", parameters)}";
        }
    }

    public class StoredObjectParameter
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public string ParameterName { get; set; }
        public bool IsOutput { get; set; }
        public bool IsNullable { get; set; }
        public bool IsNullableType => IsNullable && CSharpTypeDefinition != "string" && CSharpTypeDefinition != "object";
        public string SqlType { get; set; }
        public int Order { get; set; }
        public string CSharpTypeDefinition => SqlType.GetCSharpType();
        public string CSharpName => ParameterName.Replace("@", "").LowerCaseFirstChar();
        public string ParameterNameWithoutAt => ParameterName.Replace("@", "");
        public string Definition { get; set; }

        public override string ToString()
        {
            return $"{(IsOutput ? "OUT " : "")}{SqlType}{(IsNullable ? "?" : "")} {ParameterName}";
        }
    }

    public class StoredObjectSetColumn
    {
        public string Name { get; set; }

        public int Order { get; set; }

        public bool IsNullable { get; set; }

        public string SqlType { get; set; }

        public override string ToString()
        {
            return $"{SqlType}{(IsNullable ? "?" : "")} {Name}";
        }
    }

    public class TableValuedColumn : StoredObjectSetColumn
    {
        public string Schema { get; set; }
        public string FunctionName { get; set; }
    }
}