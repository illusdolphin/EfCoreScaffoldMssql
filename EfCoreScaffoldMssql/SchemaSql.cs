
namespace EfCoreScaffoldMssql
{
    internal static class SchemaSql
    {
        internal static string TablesSql = @"SELECT s.name as SchemaName, t.name as EntityName FROM Sys.tables t
JOIN Sys.schemas s on s.schema_id = t.schema_id";

        internal static string ViewsSql = @"SELECT s.name as SchemaName, t.name as EntityName FROM Sys.views t
JOIN Sys.schemas s on s.schema_id = t.schema_id";

        internal static string TableColumnsSql = @"select 
	c.name as [Name], 
	s.name as SchemaName, 
	t.name as ObjectName,
	typ.name as [TypeName],
	c.max_length as [MaxLength],
	c.is_computed as IsComputed,
	c.is_nullable as IsNullable,
	c.is_identity as IsIdentity,
    object_definition(c.default_object_id) AS DefaultDefinition,
    cc.definition as ComputedColumnSql,
    c.column_id as ColumnId
from Sys.columns c
JOIN Sys.tables t on t.object_id = c.object_id
JOIN Sys.schemas s on s.schema_id = t.schema_id
JOIN Sys.types typ on c.user_type_id = typ.user_type_id
LEFT OUTER JOIN Sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id";

        internal static string ViewColumnsSql = @"select 
	c.name as [Name], 
	s.name as SchemaName, 
	v.name as ObjectName,
	typ.name as [TypeName],
	c.max_length as [MaxLength],
	c.is_computed as IsComputed,
	c.is_nullable as IsNullable,
	c.is_identity as IsIdentity,
    object_definition(c.default_object_id) AS DefaultDefinition,
    cc.definition as ComputedColumnSql,
    c.column_id as ColumnId
from Sys.columns c
JOIN Sys.views v on v.object_id = c.object_id
JOIN Sys.schemas s on s.schema_id = v.schema_id
JOIN Sys.types typ on c.user_type_id = typ.user_type_id
LEFT OUTER JOIN Sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id";

        internal static string ForeignKeysSql = @"SELECT RC.CONSTRAINT_NAME FkName 
, KF.TABLE_SCHEMA FkSchema
, KF.TABLE_NAME FkTable
, KF.COLUMN_NAME FkColumn
, TC.CONSTRAINT_NAME PkName
, TC.TABLE_SCHEMA PkSchema
, TC.TABLE_NAME PkTable
, TU.COLUMN_NAME PkColumn
, RC.MATCH_OPTION MatchOption
, RC.UPDATE_RULE UpdateRule
, RC.DELETE_RULE DeleteRule
, KP.ORDINAL_POSITION AS PkOrdinalPosition
, KF.ORDINAL_POSITION AS FkOrdinalPosition
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC 
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KF ON RC.CONSTRAINT_NAME = KF.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KP ON RC.CONSTRAINT_NAME = KP.CONSTRAINT_NAME AND KF.ORDINAL_POSITION = KP.ORDINAL_POSITION
JOIN sys.indexes SI on SI.[Name] = RC.UNIQUE_CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC on TC.CONSTRAINT_SCHEMA = OBJECT_SCHEMA_NAME(SI.[object_id]) AND TC.TABLE_NAME = OBJECT_NAME(SI.[object_id]) AND TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
INNER HASH JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE TU on TC.CONSTRAINT_NAME = TU.CONSTRAINT_NAME AND TU.ORDINAL_POSITION = KF.ORDINAL_POSITION
ORDER BY KP.TABLE_NAME, KP.CONSTRAINT_NAME";

        internal static string KeyColumnsSql = @"SELECT 
  s.name as TableSchema,
  t.name as TableName,
  c.name as ColumnName,
  i.name as KeyName,
  ic.key_ordinal as KeyOrder
from sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.columns c ON c.object_id = t.object_id
JOIN sys.index_columns ic ON ic.object_id = c.object_id AND ic.column_id = c.column_id
JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
WHERE i.is_primary_key = 1";

        internal static string DefaultSchemaSql = @"SELECT SCHEMA_NAME() AS SchemaName";

    }
}
