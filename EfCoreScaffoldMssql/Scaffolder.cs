using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EfCoreScaffoldMssql.Classes;
using EfCoreScaffoldMssql.Helpers;
using HandlebarsDotNet;

namespace EfCoreScaffoldMssql
{
    public class Scaffolder
    {
        private static readonly Regex removeIdRegex = new Regex("(?<content>.+)(Id)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly ScaffoldOptions _options;

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

        public void ScaffoldObjects(
            List<EntityViewModel> entities,
            List<EntityDefinition> objects, 
            List<ColumnDefinition> columns, 
            List<KeyColumnDefinition> keyColumns,
            List<FkDefinition> fkDefinitions,
            List<string> ignoreObjects)
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
                    .ToList();

                var tableFks = fkDefinitions
                    .Where(x => x.FkSchema == table.SchemaName && x.FkTable == table.EntityName)
                    .ToList();

                entityViewModel.Keys = tableKeys;

                foreach (var tableColumn in tableColumns.OrderBy(x => x.ColumnId))
                {
                    var columnViewModel = tableColumn.CloneCopy<ColumnDefinition, ColumnViewModel>();

                    var keyIndex = tableKeys.FindIndex(x => x.ColumnName == tableColumn.Name);
                    columnViewModel.IsKey = keyIndex > -1;
                    columnViewModel.KeyColumnNumber = keyIndex + 1;

                    var hasFkDefinition = tableFks.Any(x => x.FkColumn == tableColumn.Name);
                    columnViewModel.IsForeignKey = hasFkDefinition;

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
            
            WriteLine("Templates are ready");

            using (var connection = new SqlConnection(_options.ConnectionString))
            {
                connection.Open();
                WriteLine("Connected to the database");

                var fkDefinitions = connection.ReadObjects<FkDefinition>(SchemaSql.ForeignKeysSql);
                WriteLine("Foreign keys information received");

                var keyColumns = connection.ReadObjects<KeyColumnDefinition>(SchemaSql.KeyColumnsSql);
                WriteLine("Primary keys information received");

                var tables = connection.ReadObjects<EntityDefinition>(SchemaSql.TablesSql);
                WriteLine("Tables information received");

                var views = connection.ReadObjects<EntityDefinition>(SchemaSql.ViewsSql);
                WriteLine("Views information received");

                var tablesColumns = connection.ReadObjects<ColumnDefinition>(SchemaSql.TableColumnsSql);
                WriteLine("Tables columns information received");

                var viewsColumns = connection.ReadObjects<ColumnDefinition>(SchemaSql.ViewColumnsSql);
                WriteLine("Views columns information received");

                var entityViewModels = new List<EntityViewModel>();

                ScaffoldObjects(entityViewModels, tables, tablesColumns, keyColumns, fkDefinitions, _options.IgnoreTables);

                ScaffoldObjects(entityViewModels, views, viewsColumns, null, null, _options.IgnoreViews);

                foreach (var foreignKey in fkDefinitions)
                {
                    var originTable = entityViewModels.SingleOrDefault(x =>
                        x.SchemaName == foreignKey.PkSchema && x.EntityName == foreignKey.PkTable);

                    var foreignTable = entityViewModels.SingleOrDefault(x =>
                        x.SchemaName == foreignKey.FkSchema && x.EntityName == foreignKey.FkTable);

                    if (originTable != null && foreignTable != null)
                    {
                        var fkColumn = foreignKey.FkColumn;

                        var propertyName = removeIdRegex.Replace(fkColumn, m => m.Groups["content"].Value).TrimEnd('_');
                        if (_options.ForeignPropertyRegex != null)
                        {
                            propertyName = Regex.Match(foreignKey.FkName, _options.ForeignPropertyRegex, RegexOptions.Singleline).Groups["PropertyName"].Value;
                            propertyName = propertyName.Replace("_", string.Empty);
                            if (propertyName.EndsWith("Id") || propertyName.EndsWith("ID"))
                            {
                                propertyName = propertyName.Substring(0, propertyName.Length - 2);
                            }
                        }

                        var inversePropertyName = propertyName.ReplaceFirstOccurrance(originTable.EntityName, foreignTable.EntityName);
                        inversePropertyName = StringHelper.Pluralize(inversePropertyName);

                        if (originTable == foreignTable)
                        {
                            inversePropertyName = "Inverse" + propertyName;
                        }

                        var foreignKeyViewModel = foreignKey.CloneCopy<FkDefinition, ForeignKeyViewModel>();
                        foreignKeyViewModel.PropertyName = propertyName;
                        foreignKeyViewModel.InversePropertyName = inversePropertyName;
                        foreignTable.ForeignKeys.Add(foreignKeyViewModel);

                        var InverseKeyViewModel = foreignKey.CloneCopy<FkDefinition, ForeignKeyViewModel>();
                        InverseKeyViewModel.PropertyName = inversePropertyName;
                        InverseKeyViewModel.InversePropertyName = propertyName;
                        originTable.InverseKeys.Add(InverseKeyViewModel);
                    }
                }

                var modelsDirectory = Path.Combine(_options.Directory, _options.ModelsPath);
                Directory.CreateDirectory(modelsDirectory);

                foreach (var tableViewModel in entityViewModels)
                {
                    var setResult = templateSet(tableViewModel);
                    File.WriteAllText(Path.Combine(modelsDirectory, tableViewModel.EntityName + ".cs"), setResult);
                }

                var contextViewModel = new ContextViewModel
                {
                    ContextName = _options.ContextName,
                    Namespace = _options.Namespace,
                    Entities = entityViewModels
                };
                var contextResult = templateContext(contextViewModel);
                File.WriteAllText(Path.Combine(modelsDirectory, contextViewModel.ContextName + ".cs"), contextResult);
            }
        }
    }
}