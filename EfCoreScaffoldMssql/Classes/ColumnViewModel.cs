﻿using EfCoreScaffoldMssql.Enums;
using EfCoreScaffoldMssql.Helpers;

namespace EfCoreScaffoldMssql.Classes
{
    public class ColumnViewModel: ColumnDefinition
    {
        public EntityFrameworkVersion EntityFrameworkVersion { get; set; }

        public string DisplayName { get; set; }
        public bool IsString => CSharpType.StartsWith("string");
        public bool IsBinary => CSharpType == "byte[]";
        public string TypeName
        {
            get
            {
                if (ColumnTypeName == "decimal")
                    return $"decimal({Precision}, {Scale})";
                return ColumnTypeName;
            }
        }

        public bool HasLengthLimit => MaxLength > 0 && !(TypeName == "ntext" || TypeName == "text");
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

        public bool NeedColumnDefinition 
        { 
            get 
            {
                return !string.Equals(Name, DisplayName);
            }
        }
        public bool NeedTypeDefinition
        {
            get
            {
                switch (ColumnTypeName)
                {
                    case "date":
                    case "datetime":
                    case "datetime2":
                    case "geometry":
                    case "smalldatetime":
                    case "decimal" when EntityFrameworkVersion == EntityFrameworkVersion.EfCore6:
                        return true;
                }

                return !string.IsNullOrWhiteSpace(ExtendedPropertiesTypeName);
            }
        }

        public bool EnableReferenceNullableTypes { get; set; }

        public string CSharpType
        {
            get
            {
                var typeDef = CSharpTypeDefinition;

                if (!EnableReferenceNullableTypes)
                { 
                    switch (typeDef)
                    {
                        case "string":
                        case "byte[]":
                        case "object":
                            return typeDef;
                    }
                }

                if (IsNullable && (TypeName != "geometry" || EnableReferenceNullableTypes))
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

                return ColumnTypeName.GetCSharpType();
            }
        }
    }
}