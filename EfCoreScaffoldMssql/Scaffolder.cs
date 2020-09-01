using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EfCoreScaffoldMssql.Classes;
using EfCoreScaffoldMssql.Helpers;
using HandlebarsDotNet;
using Newtonsoft.Json.Linq;

namespace EfCoreScaffoldMssql
{
    public class Scaffolder
    {
        private static readonly Regex RemoveIdRegex = new Regex("(?<content>.+)(Id)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly ScaffoldOptions _options;
        private static readonly Regex ColumnValidRegex = new Regex("^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        public Scaffolder(ScaffoldOptions options)
        {
            _options = options;
        }

        private void WriteLine(string message)
        {
            if (_options.IsVerbose)
            {
                Console.WriteLine(message);
            }
        }

        private void AddDependencies(EntityViewModel entity, List<ColumnDefinition> columns)
        {
            if (columns.Any(x => x.TypeName == "geometry"))
            {
                entity.Dependencies.Add(new Dependency
                {
                    Name = "NetTopologySuite.Geometries"
                });
            }
        }

        public void ScaffoldEntities(
            List<EntityViewModel> entities,
            List<EntityDefinition> objects, 
            List<ColumnDefinition> columns, 
            List<KeyColumnDefinition> keyColumns,
            List<FkDefinition> fkDefinitions,
            List<string> ignoreObjects,
            List<ObjectColumnsSettingModel> objectsColumnsSettings,
            string defaultSchemaName)
        {
            keyColumns = keyColumns ?? new List<KeyColumnDefinition>();
            fkDefinitions = fkDefinitions ?? new List<FkDefinition>();

            foreach (var table in objects.OrderBy(x => x.EntityName))
            {
                if (_options.Schemas.Any() && !_options.Schemas.Contains(table.SchemaName.ToLower()))
                    continue;

                if (ignoreObjects.Contains($"[{table.SchemaName}].[{table.EntityName}]".ToLower()))
                    continue;

                var entityViewModel = table.CloneCopy<EntityDefinition, EntityViewModel>();

                entityViewModel.Namespace = _options.Namespace;

                var tableColumns = columns
                    .Where(x => x.SchemaName == table.SchemaName && x.ObjectName == table.EntityName)
                    .ToList();

                var tableKeys = keyColumns
                    .Where(x => x.TableSchema == table.SchemaName && x.TableName == table.EntityName)
                    .OrderBy(x => x.KeyOrder)
                    .ToList();

                var tableFks = fkDefinitions
                    .Where(x => x.FkSchema == table.SchemaName && x.FkTable == table.EntityName)
                    .ToList();


                if (objectsColumnsSettings != null)
                {
                    var objectColumnsSetting = objectsColumnsSettings.Find(x => x.ObjectName == table.EntityName);
                    if (objectColumnsSetting != null)
                    {
                        var columnsSetting = objectColumnsSetting.ColumnsList.Where(x => x.NewName == null).Select(x => x.Name).ToList();
                        tableColumns.RemoveAll(x => columnsSetting.Contains(x.Name));
                    }
                }

                var keyViewModels = new List<KeyColumnViewModel>();
                foreach (var tableKey in tableKeys)
                {
                    var keyViewModel = tableKey.CloneCopy<KeyColumnDefinition, KeyColumnViewModel>();
                    keyViewModel.ColumnDisplayName = PropertyHelper.GetColumnNameToDisplay(keyViewModel.ColumnName, table.EntityName, objectsColumnsSettings);
                    keyViewModels.Add(keyViewModel);
                }

                entityViewModel.Keys = keyViewModels;
                entityViewModel.IsDefaultSchema = defaultSchemaName == entityViewModel.SchemaName;

                AddDependencies(entityViewModel, tableColumns);

                foreach (var tableColumn in tableColumns.OrderBy(x => x.ColumnId))
                {
                    var columnViewModel = tableColumn.CloneCopy<ColumnDefinition, ColumnViewModel>();

                    var keyIndex = tableKeys.FindIndex(x => x.ColumnName == tableColumn.Name);
                    columnViewModel.IsKey = keyIndex > -1;
                    columnViewModel.KeyColumnNumber = keyIndex + 1;

                    var hasFkDefinition = tableFks.Any(x => x.FkColumns.Contains(tableColumn.Name));
                    columnViewModel.IsPartOfForeignKey = hasFkDefinition;

                    columnViewModel.DisplayName = PropertyHelper.GetColumnNameToDisplay(tableColumn.Name, table.EntityName, objectsColumnsSettings);

                    entityViewModel.Columns.Add(columnViewModel);
                }

                entities.Add(entityViewModel);
            }
        }

        public void Generate()
        {
            const string setFileName = "set.hbs";
            const string contextFileName = "context.hbs";
            Func<object, string> templateSet;
            Func<object, string> templateContext;
            var fksPresetList = new List<FkPresetDefinition>();
            var tablesColumnsSettingsList = new List<ObjectColumnsSettingModel>();
            var viewsColumnsSettingsList = new List<ObjectColumnsSettingModel>();
            var fkPropertyDisplayNamesSettingsList = new List<FkPropertyDisplayNameDefinition>();
            Handlebars.RegisterHelper("IfColumnsContainsAll", HBSHelper.IfColumnsContainsAll);

            try
            {
                var setTemplate = File.ReadAllText(Path.Combine(_options.TemplatesDirectory, setFileName));
                templateSet = Handlebars.Compile(setTemplate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error compiling Handlebars template {setFileName}: {ex.Message}");
                return;
            }
            try
            {
                var tableContext = File.ReadAllText(Path.Combine(_options.TemplatesDirectory, contextFileName));
                templateContext = Handlebars.Compile(tableContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error compiling Handlebars template {contextFileName}: {ex.Message}");
                return;
            }

            if (!string.IsNullOrEmpty(_options.CustomSettingsJsonPath))
            {
                try
                {
                    var customSettingsJsonString = Path.Combine(_options.CustomSettingsJsonPath);
                    var customSettingsJsonObject = JObject.Parse(File.ReadAllText(customSettingsJsonString));
                    var foreignKeys = (JObject)customSettingsJsonObject.GetValue("ForeignKeys");
                    var tablesColumnsSettings = (JObject)customSettingsJsonObject.GetValue("TablesColumns");
                    var viewsColumnsSettings = (JObject)customSettingsJsonObject.GetValue("ViewsColumns");
                    var foreignKeyPropertyDisplayNames = (JArray)customSettingsJsonObject.GetValue("FKPropertyNames");

                    fksPresetList = GetForeignKeysPresetList(foreignKeys);
                    tablesColumnsSettingsList = GetObjectsColumnsSettingsList(tablesColumnsSettings);
                    viewsColumnsSettingsList = GetObjectsColumnsSettingsList(viewsColumnsSettings);
                    fkPropertyDisplayNamesSettingsList = GetFkPropertyDisplayNamesSettingsList(foreignKeyPropertyDisplayNames);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading CustomSettings json file: {ex.Message}");
                    return;
                }
            }

            WriteLine("Templates are ready");

            var fkNamesSkipList = fksPresetList.Where(x => x.FkPropertyNames == null).Select(x => x.ForeignKeyName).ToList();


            using (var connection = new SqlConnection(_options.ConnectionString))
            {
                connection.Open();
                WriteLine("Connected to the database");

                var fkDefinitionsSource = connection.ReadObjects<FkDefinitionSource>(SchemaSql.ForeignKeysSql);
                var fkDefinitions =
                    (from s in fkDefinitionsSource.Where(x => !fkNamesSkipList.Contains(x.FkName))
                    group s by new {s.PkSchema, s.FkSchema, s.FkTable, s.PkTable, s.FkName, s.PkName, s.MatchOption, s.UpdateRule, s.DeleteRule}
                    into sGroup
                    select new FkDefinition
                    {
                        PkSchema = sGroup.Key.PkSchema,
                        FkSchema = sGroup.Key.FkSchema,
                        PkTable = sGroup.Key.PkTable,
                        FkTable = sGroup.Key.FkTable,
                        PkName = sGroup.Key.PkName,
                        FkName = sGroup.Key.FkName,
                        MatchOption = sGroup.Key.MatchOption,
                        DeleteRule = sGroup.Key.DeleteRule,
                        UpdateRule = sGroup.Key.UpdateRule,
                        PkColumns = sGroup.OrderBy(x => x.PkOrdinalPosition).Select(x => x.PkColumn).ToList(),
                        PkColumnDisplayNames = sGroup.OrderBy(x => x.PkOrdinalPosition).Select(x => PropertyHelper.GetColumnNameToDisplay(x.PkColumn, sGroup.Key.PkTable, tablesColumnsSettingsList)).ToList(),
                        FkColumns = sGroup.OrderBy(x => x.FkOrdinalPosition).Select(x => x.FkColumn).ToList(),
                        FkColumnDisplayNames = sGroup.OrderBy(x => x.FkOrdinalPosition).Select(x => PropertyHelper.GetColumnNameToDisplay(x.FkColumn, sGroup.Key.FkTable, tablesColumnsSettingsList)).ToList()
                    }).ToList();

                WriteLine("Foreign keys information received");

                var keyColumns = connection.ReadObjects<KeyColumnDefinition>(SchemaSql.KeyColumnsSql);
                WriteLine("Primary keys information received");

                var tables = connection.ReadObjects<EntityDefinition>(SchemaSql.TablesSql);
                WriteLine("Tables information received");

                var views = connection.ReadObjects<EntityDefinition>(SchemaSql.ViewsSql);
                WriteLine("Views information received");

                var tablesColumns = connection.ReadObjects<ColumnDefinition>(string.Format(SchemaSql.TableColumnsSql, _options.ExtendedPropertyTypeName));
                WriteLine("Tables columns information received");

                var viewsColumns = connection.ReadObjects<ColumnDefinition>(string.Format(SchemaSql.ViewColumnsSql, _options.ExtendedPropertyTypeName));
                WriteLine("Views columns information received");

                var spDefinitions = new List<StoredObjectDefinition>();
                if (_options.GenerateStoredProcedures)
                {
                    spDefinitions = GetStoredObjectsDefinition(connection, SchemaSql.StoredProcedureParametersSql, false);
                    WriteLine("Stored procedures parameters information received");

                    foreach (var sp in spDefinitions)
                    {
                        WriteLine($"Reading schema for {sp.Schema}.{sp.Name}");
                        var spSetDefinition = string.Format(SchemaSql.StoredProcedureSetSql, sp.Schema, sp.Name);
                        var columns = connection.ReadObjects<StoredObjectSetColumn>(spSetDefinition);
                        sp.Columns = columns.Where(x => !string.IsNullOrEmpty(x.Name) && ColumnValidRegex.IsMatch(x.Name)).ToList();
                    }
                }

                var tvfDefinitions = new List<StoredObjectDefinition>();

                if (_options.GenerateTableValuedFunctions)
                {
                    tvfDefinitions = GetStoredObjectsDefinition(connection, SchemaSql.TableValueFunctionParametersSql, true);
                    WriteLine("Table valued functions parameters information received");

                    var tvfColumns = connection.ReadObjects<TableValuedColumn>(SchemaSql.TableValueFunctionColumnsSql);
                    WriteLine("Table valued functions parameters information received");
                    foreach (var tvf in tvfDefinitions)
                    {
                        tvf.Columns = tvfColumns
                            .Where(c => c.Schema == tvf.Schema && c.FunctionName == tvf.Name)
                            .Cast<StoredObjectSetColumn>().ToList();
                    }
                }

                var defaultSchemaName = connection.ReadObjects<SchemaDefinition>(SchemaSql.DefaultSchemaSql).First().SchemaName;

                var entityViewModels = new List<EntityViewModel>();

                ScaffoldEntities(entityViewModels, tables, tablesColumns, keyColumns, fkDefinitions, _options.IgnoreTables, tablesColumnsSettingsList, defaultSchemaName);

                ScaffoldEntities(entityViewModels, views, viewsColumns, null, null, _options.IgnoreViews, viewsColumnsSettingsList, defaultSchemaName);

                var pKeys =
                    (from pk in keyColumns
                        group pk by new {pk.TableSchema, pk.TableName, pk.KeyName}
                        into pkGroup
                        select new
                        {
                            pkGroup.Key.TableSchema,
                            pkGroup.Key.TableName,
                            Columns = pkGroup.OrderBy(x => x.KeyOrder).Select(x => x.ColumnName).ToList()
                        }).ToDictionary(x => $"{x.TableSchema}.{x.TableName}", x => x.Columns);

                foreach (var foreignKey in fkDefinitions)
                {
                    
                    var originTable = entityViewModels.SingleOrDefault(x =>
                        x.SchemaName == foreignKey.PkSchema && x.EntityName == foreignKey.PkTable);

                    var foreignTable = entityViewModels.SingleOrDefault(x =>
                        x.SchemaName == foreignKey.FkSchema && x.EntityName == foreignKey.FkTable);

                    var isOneToOne = false;
                    //Check one-2-one in case matched columns names and theirs orders
                    var originTableFullName = $"{foreignKey.PkSchema}.{foreignKey.PkTable}";
                    var foreignTableFullName = $"{foreignKey.FkSchema}.{foreignKey.FkTable}";
                    if (pKeys.ContainsKey(originTableFullName) && pKeys.ContainsKey(foreignTableFullName))
                    {
                        var pKeyOrigin = pKeys[originTableFullName];
                        var pKeyForeign = pKeys[foreignTableFullName];
                        if (foreignKey.PkColumns.Count == foreignKey.FkColumns.Count && foreignKey.PkColumns.Count == pKeyOrigin.Count && foreignKey.PkColumns.Count == pKeyForeign.Count)
                        {
                            isOneToOne = true;
                            for (var i = 0; i < pKeyOrigin.Count; i++)
                            {
                                if (pKeyOrigin[i] == foreignKey.PkColumns[i] && pKeyForeign[i] == foreignKey.FkColumns[i])
                                {
                                    continue;
                                }
                                isOneToOne = false;
                                break;
                            }
                        }
                    }

                    if (originTable != null && foreignTable != null)
                    {
                        var propertyName = string.Empty;
                        var inversePropertyName = string.Empty;

                        var fkPreset = fksPresetList.FirstOrDefault(x => x.ForeignKeyName == foreignKey.FkName && x.FkPropertyNames != null);
                        var hasFKPreset = fkPreset != null && fkPreset.FkPropertyNames != null;

                        if (hasFKPreset)
                        {
                            propertyName = fkPreset.FkPropertyNames.PropertyName;
                            inversePropertyName = fkPreset.FkPropertyNames.InversePropertyName;
                        }

                        if (string.IsNullOrEmpty(propertyName))
                        {
                            foreach (var fkColumn in foreignKey.FkColumns)
                            {
                                propertyName = RemoveIdRegex.Replace(fkColumn, m => m.Groups["content"].Value).TrimEnd('_');
                            }

                            if (_options.ForeignPropertyRegex != null)
                            {
                                propertyName = Regex.Match(foreignKey.FkName, _options.ForeignPropertyRegex, RegexOptions.Singleline).Groups["PropertyName"].Value;
                                propertyName = propertyName.Replace("_", string.Empty);
                                if (propertyName.EndsWith("Id") || propertyName.EndsWith("ID"))
                                {
                                    propertyName = propertyName.Substring(0, propertyName.Length - 2);
                                }
                            }
                        }

                        if (fkPropertyDisplayNamesSettingsList != null && fkPropertyDisplayNamesSettingsList.Count > 0 && !hasFKPreset)
                        {
                            var fkPropertyDisplayNameSetting = fkPropertyDisplayNamesSettingsList.Find(x => x.Name == propertyName);
                            if (fkPropertyDisplayNameSetting != null && !string.IsNullOrEmpty(fkPropertyDisplayNameSetting.DisplayName))
                            {
                                propertyName = fkPropertyDisplayNameSetting.DisplayName;
                                inversePropertyName = propertyName.ReplaceFirstOccurrance(originTable.EntityName, foreignTable.EntityName, true);
                            }
                        }

                        propertyName = PropertyHelper.GetColumnNameToDisplay(propertyName, originTable.EntityName, tablesColumnsSettingsList);

                        if (string.IsNullOrEmpty(inversePropertyName))
                        {
                            inversePropertyName = propertyName.ReplaceFirstOccurrance(originTable.EntityName, foreignTable.EntityName);
                        }

                        if (!isOneToOne)
                        {
                            inversePropertyName = StringHelper.Pluralize(inversePropertyName);
                        }

                        if (originTable == foreignTable)
                        {
                            inversePropertyName = "Inverse" + propertyName;
                        }

                        var keyCount = originTable.InverseKeys.Count(x => x.PropertyName == inversePropertyName);
                        var propName = keyCount == 0 ? propertyName : $"{propertyName}_{keyCount}";
                        var inversePropName = keyCount == 0 ? inversePropertyName : $"{inversePropertyName}_{keyCount}";

                        var foreignKeyViewModel = foreignKey.CloneCopy<FkDefinition, ForeignKeyViewModel>();
                        foreignKeyViewModel.PropertyName = propName;
                        foreignKeyViewModel.InversePropertyName = inversePropName;
                        foreignKeyViewModel.InverseEntityName = originTable.EntityName;
                        foreignKeyViewModel.IsOneToOne = isOneToOne;
                        foreignTable.ForeignKeys.Add(foreignKeyViewModel);
                        
                        var inverseKeyViewModel = foreignKey.CloneCopy<FkDefinition, ForeignKeyViewModel>();
                        inverseKeyViewModel.PropertyName = inversePropName;
                        inverseKeyViewModel.InversePropertyName = propName;
                        foreignKeyViewModel.InverseEntityName = foreignTable.EntityName;
                        inverseKeyViewModel.IsOneToOne = isOneToOne;
                        originTable.InverseKeys.Add(inverseKeyViewModel);
                    }
                }

                var fileNames = new List<string>();
                var modelsDirectory = Path.Combine(_options.Directory, _options.ModelsPath);
                Directory.CreateDirectory(modelsDirectory);

                foreach (var tableViewModel in entityViewModels)
                {
                    var setResult = templateSet(tableViewModel);
                    var setResultFileName = Path.Combine(modelsDirectory, tableViewModel.EntityName + ".cs");
                    File.WriteAllText(setResultFileName, setResult);

                    fileNames.Add(setResultFileName);
                }

                if (_options.GenerateStoredProcedures)
                {
                    WriteObjectSets(spDefinitions, modelsDirectory, templateSet, fileNames, tablesColumnsSettingsList);
                }

                if (_options.GenerateTableValuedFunctions)
                {
                    WriteObjectSets(tvfDefinitions, modelsDirectory, templateSet, fileNames, tablesColumnsSettingsList);
                }

                var contextViewModel = new ContextViewModel
                {
                    ContextName = _options.ContextName,
                    Namespace = _options.Namespace,
                    Entities = entityViewModels,
                    StoredProcedures = spDefinitions,
                    TableValuedFunctions = tvfDefinitions
                };
                var contextResult = templateContext(contextViewModel);
                var contextResultFileName = Path.Combine(modelsDirectory, contextViewModel.ContextName + ".cs");
                File.WriteAllText(contextResultFileName, contextResult);

                fileNames.Add(contextResultFileName);

                if (_options.CleanUp)
                {
                    var directoryFiles = Directory.GetFiles(modelsDirectory, "*.cs");

                    var filesToCleanUp = directoryFiles.Except(fileNames);

                    foreach (var s in filesToCleanUp)
                    {
                        File.Delete(s);
                    }
                }
            }
        }

        private List<StoredObjectDefinition> GetStoredObjectsDefinition(SqlConnection connection, string sql, bool isFunction)
        {
            var storedProcedureParameters = connection.ReadObjects<StoredObjectParameter>(sql);
            var objectDefinitions = (from p in storedProcedureParameters
                group p by new { p.Schema, p.Name }
                into sGroup
                select new StoredObjectDefinition
                {
                    Schema = sGroup.Key.Schema,
                    Name = sGroup.Key.Name,
                    IsFunction = isFunction,
                    Parameters = sGroup.Where(p => !string.IsNullOrEmpty(p.ParameterName)).Select(p => new StoredObjectParameter
                    {
                        ParameterName = p.ParameterName,
                        Schema = sGroup.Key.Schema,
                        Name = sGroup.Key.Name,
                        Order = p.Order,
                        IsOutput = p.IsOutput,
                        IsNullable = p.IsNullable,
                        SqlType = p.SqlType
                    }).OrderBy(p => p.Order).ToList()
                }).ToList();
            return objectDefinitions;
        }

        private void WriteObjectSets(IEnumerable<StoredObjectDefinition> objectDefinitions, string modelsDirectory, Func<object, string> templateSet, List<string> fileNames, List<ObjectColumnsSettingModel> objectsColumnsSettings = null)
        {
            foreach (var p in objectDefinitions.Where(x => x.Columns.Count > 0))
            {
                var model = new EntityViewModel
                {
                    SchemaName = p.Schema,
                    EntityName = p.ResultTypeName,
                    Namespace = _options.Namespace,
                    IsVirtual = true,
                    Columns = p.Columns.Select(c => new ColumnViewModel
                    {
                        SchemaName = p.Schema,
                        Name = c.Name,
                        TypeName = c.SqlType,
                        IsNullable = c.IsNullable,
                        DisplayName = PropertyHelper.GetColumnNameToDisplay(c.Name, p.ResultTypeName, objectsColumnsSettings)
                    }).ToList()
                };

                var setResult = templateSet(model);
                var setResultFileName = Path.Combine(modelsDirectory, p.ResultTypeName + ".cs");
                File.WriteAllText(setResultFileName, setResult);

                fileNames.Add(setResultFileName);
            }
        }

        private List<ObjectColumnsSettingModel> GetObjectsColumnsSettingsList(JObject objectsColumns)
        {
            var objectsColumnsSettingsList = new List<ObjectColumnsSettingModel>();

            if (objectsColumns != null)
            { 
                foreach (var objectColumns in objectsColumns)
                {
                    if (!string.IsNullOrEmpty(objectColumns.Key) && objectColumns.Value.HasValues)
                    {
                        var objectColumnsSetting = new ObjectColumnsSettingModel()
                        {
                            ObjectName = objectColumns.Key,
                            ColumnsList = new List<ColumnSettingModel>()
                        };

                        var columns = objectColumns.Value;

                        foreach (var column in columns)
                        {
                            var columnSetting = column.ToObject<ColumnSettingModel>();
                            objectColumnsSetting.ColumnsList.Add(columnSetting);
                        }

                        objectsColumnsSettingsList.Add(objectColumnsSetting);
                    }
                }
            }
            return objectsColumnsSettingsList;
        }

        private List<FkPresetDefinition> GetForeignKeysPresetList(JObject foreignKeys)
        {
            var fksPresetList = new List<FkPresetDefinition>();

            if (foreignKeys != null)
            {
                foreach (var foreignKey in foreignKeys)
                {
                    if (!string.IsNullOrEmpty(foreignKey.Key))
                    {
                        var fkPresetDefinition = new FkPresetDefinition();
                        fkPresetDefinition.ForeignKeyName = foreignKey.Key;
                        if (foreignKey.Value.HasValues)
                        {
                            fkPresetDefinition.FkPropertyNames = foreignKey.Value.ToObject<FkPropertyNameDefinition>();
                        }
                        else
                        {
                            fkPresetDefinition.FkPropertyNames = null;
                        }
                        fksPresetList.Add(fkPresetDefinition);
                    }
                }
            }
            return fksPresetList;
        }

        private List<FkPropertyDisplayNameDefinition> GetFkPropertyDisplayNamesSettingsList(JArray fkPropertyDisplayNames)
        {
            var fkPropertyDisplayNamesSettingsList = new List<FkPropertyDisplayNameDefinition>();

            if (fkPropertyDisplayNames != null)
            {
                fkPropertyDisplayNamesSettingsList = fkPropertyDisplayNames.ToObject<List<FkPropertyDisplayNameDefinition>>();
            }
            return fkPropertyDisplayNamesSettingsList;
        }
    }
}