
namespace EfCoreScaffoldMssql
{
    internal static class SchemaSql
    {
        internal const string TablesSql = @"SELECT s.name as SchemaName, t.name as EntityName, NULL as Definition, convert(bit, 0) AS IsViewEntity FROM Sys.tables t
JOIN Sys.schemas s on s.schema_id = t.schema_id";

        internal const string ViewsSql = @"SELECT s.name as SchemaName, t.name as EntityName,
(SELECT OBJECT_DEFINITION(OBJECT_ID(s.[name]+'.'+t.[name]) )) as Definition, convert(bit, 1) AS IsViewEntity FROM Sys.views t
JOIN Sys.schemas s on s.schema_id = t.schema_id";

        internal const string TableColumnsSql = @"select 
	c.name as [Name], 
	s.name as SchemaName, 
	t.name as ObjectName,
	typ.name as [ColumnTypeName],
	c.max_length as [MaxLength],
    c.[precision] as [Precision],
	c.scale as [Scale],
	c.is_computed as IsComputed,
	c.is_nullable as IsNullable,
	c.is_identity as IsIdentity,
    object_definition(c.default_object_id) AS DefaultDefinition,
    cc.definition as ComputedColumnSql,
    c.column_id as ColumnId,
    p.value as ExtendedPropertiesTypeName
from Sys.columns c
JOIN Sys.tables t on t.object_id = c.object_id
JOIN Sys.schemas s on s.schema_id = t.schema_id
JOIN Sys.types typ on c.user_type_id = typ.user_type_id
LEFT OUTER JOIN Sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
LEFT JOIN sys.extended_properties AS p ON p.major_id=t.object_id AND p.minor_id=c.column_id AND p.class=1 AND p.name = '{0}'
";

        internal const string TriggersSql = @"SELECT 
	s.name AS [TableSchema],
	t.name AS [TableName],
	tr.name AS [TriggerName]
FROM sys.triggers tr
JOIN sys.tables t ON t.object_id = tr.parent_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
ORDER BY [TableSchema], [TableName], [TriggerName]
";

        internal const string ViewColumnsSql = @"select 
	c.name as [Name], 
	s.name as SchemaName, 
	v.name as ObjectName,
	typ.name as [ColumnTypeName],
	c.max_length as [MaxLength],
    c.[precision] as [Precision],
	c.scale as [Scale],
	c.is_computed as IsComputed,
	c.is_nullable as IsNullable,
	c.is_identity as IsIdentity,
    object_definition(c.default_object_id) AS DefaultDefinition,
    cc.definition as ComputedColumnSql,
    c.column_id as ColumnId,
    p.value as ExtendedPropertiesTypeName
from Sys.columns c
JOIN Sys.views v on v.object_id = c.object_id
JOIN Sys.schemas s on s.schema_id = v.schema_id
JOIN Sys.types typ on c.user_type_id = typ.user_type_id
LEFT OUTER JOIN Sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
LEFT JOIN sys.extended_properties AS p ON p.major_id=v.object_id AND p.minor_id=c.column_id AND p.class=1 AND p.name = '{0}'
";

        internal const string ForeignKeysSql = @"SELECT RC.CONSTRAINT_NAME FkName 
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

        internal const string KeyColumnsSql = @"SELECT 
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
WHERE i.is_primary_key = 1
ORDER BY [TableSchema], [TableName], [KeyName], [KeyOrder]";

        internal const string DefaultSchemaSql = @"SELECT SCHEMA_NAME() AS SchemaName";

        internal const string StoredProcedureParametersSql = @"SELECT  
s.[name] AS [Schema],
p.[name] AS [Name],
par.[name] AS [ParameterName],
par.is_output AS [IsOutput],
par.is_nullable AS [IsNullable],
type_name(user_type_id) AS [SqlType],  
max_length AS [MaxLength],  
CASE WHEN type_name(system_type_id) = 'uniqueidentifier' THEN par.[precision] ELSE OdbcPrec(system_type_id, max_length, par.[precision]) END AS [Precision],  
OdbcScale(system_type_id, scale) AS [Scale],  
parameter_id AS [Order],  
CONVERT(sysname, CASE WHEN system_type_id in (35, 99, 167, 175, 231, 239) THEN ServerProperty('collation') END) AS [Collation],
(SELECT OBJECT_DEFINITION(OBJECT_ID(s.[name]+'.'+p.[name]) )) as Definition
FROM sys.procedures p
LEFT JOIN sys.parameters par on par.object_id = p.object_id
JOIN sys.schemas s ON s.schema_id = p.schema_id
ORDER BY [Schema], [Name], [Order]";

        internal const string StoredProcedureSetSql = @"SELECT 
name AS [Name],
column_ordinal AS [Order],
is_nullable AS [IsNullable],
type_name(system_type_id) AS [SqlType],
[precision] as [Precision],
scale as [Scale],
max_length AS [MaxLength]
FROM sys.dm_exec_describe_first_result_set ('{0}.{1}', NULL, 0)
ORDER By [Order]";

        internal const string TableValueFunctionParametersSql = @"SELECT 
	SCHEMA_NAME(obj.schema_id) AS [Schema],
	obj.[name] as [Name],
	p.[name] AS [ParameterName],
    CONVERT(bit, 0) AS [IsOutput],
	p.is_nullable AS [IsNullable],
	type_name(p.system_type_id) AS [SqlType],
	p.parameter_id AS [Order],
    (SELECT OBJECT_DEFINITION(OBJECT_ID(SCHEMA_NAME(obj.schema_id)+'.'+obj.[name]) )) as Definition
FROM sys.objects obj
JOIN sys.schemas s ON s.schema_id = obj.schema_id
LEFT JOIN sys.all_parameters p on p.object_id = obj.object_id
WHERE type IN ('IF','TF')
ORDER BY [Schema], [Name], [Order]";

        internal const string TableValueFunctionColumnsSql = @"SELECT 
	SCHEMA_NAME(obj.schema_id) AS [Schema],
    obj.[name] AS [FunctionName],
	c.[name] AS [Name],
	c.is_nullable AS [IsNullable],
	type_name(c.system_type_id) AS [SqlType],
    c.[precision] as [Precision],
	c.scale as [Scale],
	c.column_id AS [Order]
FROM sys.columns c
JOIN sys.objects obj ON obj.object_id = c.object_id
JOIN sys.schemas s ON s.schema_id = obj.schema_id
WHERE obj.[type] IN ('IF','TF')
ORDER By [Schema], [Name], [Order]";
	}
}
