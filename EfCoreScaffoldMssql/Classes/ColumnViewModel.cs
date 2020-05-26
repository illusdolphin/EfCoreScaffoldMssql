using System;
using EfCoreScaffoldMssql.Helpers;

namespace EfCoreScaffoldMssql.Classes
{
    public class ColumnViewModel: ColumnDefinition
    {
        public bool IsString => CSharpType == "string";
        public bool HasLengthLimit => MaxLength != -1 && !(TypeName == "ntext" || TypeName == "text");
        public int MaxStringLength => TypeName == "nvarchar" || TypeName == "nchar" ? MaxLength / 2 : MaxLength;
        public bool IsRequiredString => !IsNullable && IsString && !IsKey;

        public bool IsKey { get; set; }
        public int KeyColumnNumber { get; set; }

        public bool IsPartOfForeignKey { get; set; }

        public bool IsNonUnicodeString => TypeName == "varchar" || TypeName == "char" || TypeName == "text";
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

                if (IsNullable && TypeName != "geometry")
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

                return TypeName.GetCSharpType();
            }
        }
    }
}