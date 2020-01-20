using System;

namespace EfCoreScaffoldMssql.Classes
{
    public class ColumnViewModel: ColumnDefinition
    {
        public bool IsString => CSharpType == "string";
        public bool HasLengthLimit => MaxLength != -1;
        public int MaxStringLength => TypeName == "nvarchar" || TypeName == "nchar" ? MaxLength / 2 : MaxLength;
        public bool IsRequiredString => !IsNullable && IsString && !IsKey;

        public bool IsKey { get; set; }
        public int KeyColumnNumber { get; set; }

        public bool IsPartOfForeignKey { get; set; }

        public bool IsNonUnicodeString => TypeName == "varchar" || TypeName == "char";
        public bool HasDefaultDefinition => !string.IsNullOrEmpty(DefaultDefinition);
        public bool HasComputedColumnSql => IsComputed && !IsKey;
        public bool IsValueGeneratedNever => IsKey && !IsIdentity && !HasDefaultDefinition && !IsString && !IsPartOfForeignKey;

        public bool HasModifiers => IsNonUnicodeString 
            || HasDefaultDefinition 
            || IsValueGeneratedNever
            || IsIdentity
			|| IsComputed;

        public bool NeedTypeDefinition
        {
            get
            {
                switch (TypeName)
                {
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "smalldatetime":
                    case "geometry":
                        return true;
                }

                return !string.IsNullOrWhiteSpace(ExtendedPropertiesTypeName);
            }
        }

        public string CSharpType
        {
            get
            {
                var typeDef = CSharpTypeDefinition;
                switch (typeDef)
                {
                    case "string":
                    case "byte[]":
                    case "object":
                        return typeDef;
                }

                if (IsNullable)
                    return typeDef + "?";

                return typeDef;
            }
        }

        public string CSharpTypeDefinition
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ExtendedPropertiesTypeName))
                {
                    return ExtendedPropertiesTypeName;
                }


                switch (TypeName)
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
                        return "string";

                    default:
                        return "object";
                }
            }
        }
    }
}