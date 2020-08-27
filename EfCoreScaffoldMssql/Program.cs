using System;
using System.Diagnostics;
using System.Linq;
using EfCoreScaffoldMssql.Helpers;
using OracleDbUpdater.Helpers;

namespace EfCoreScaffoldMssql
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                var isHelp = CommandLineHelper.HasParameterByName(args, "--help")
                             || CommandLineHelper.HasParameterByName(args, "-H")
                             || args.Length == 0;

                const string defaultNamespace = "Project.Name";
                const string defaultContext = "DatabaseContext";
                const string defaultModels = "Models";

                if (isHelp)
                {
                    Console.WriteLine("Scaffold DF core model from database (MSSQL)");
                    Console.WriteLine();
                    Console.WriteLine("Usage:");
                    Console.WriteLine("-H,--help - display this help");
                    Console.WriteLine($"-N,--namespace <Namespace> - specifies namespace for generated classes, default is '{defaultNamespace}'");
                    Console.WriteLine($"-C,--context <Name> - name for context, default is '{defaultContext}'");
                    Console.WriteLine($"-M,--models <Path> - path for models, default is '{defaultModels}'");
                    Console.WriteLine("-S,--schema <Schema> - comma-separated list of schema to include, default is not defined, meaning is to include all");
                    Console.WriteLine("-SP,--stored-procedures - comma-separated list of SPs to include, if list is empty - all are generated");
                    //TODO: Console.WriteLine("-TVF,--table-value-functions - comma-separated list of TVFs to include, if list is empty - all are generated");
                    Console.WriteLine("-IT,--ignore-tables <Tables> - comma-separated list of tables to exclude. Example: '[dbo].Table1,[master].Table2'");
                    Console.WriteLine("-IV,--ignore-views <Views> - comma-separated list of view to exclude. Example: '[dbo].View1,[master].View2'");
                    Console.WriteLine("-FR,--foreign-key-regex <regex> - Regex to extract foreign property name. Example: 'FK_([a-zA-Z]+)_(?<PropertyName>.+)'");
                    Console.WriteLine("-CS,--connectionString <ConnectionString> - Connection string to the db, example: Data Source=.;Initial Catalog=Database;integrated security=SSPI");
                    Console.WriteLine("-TD,--templates-directory <Path> - Path with template fies set.hbs and context.hbs");
                    Console.WriteLine("-ETN,--extended-property-type-name <Name> - Extended property name to read model property type from, default is 'TypeName'. If column does not have extended property then model property type is inferred from the database column type.");
                    Console.WriteLine("-V,--verbose <Schema> - Show messages during execution");
                    Console.WriteLine("-FKPD,--foreign-keys-preset-directory <Path> - Path with foreign keys preset file fks.json");
                    Console.WriteLine("-ITC,--ignore-table-columns-directory <Path> - Path with ignore table columns file ignoreTableColumns.json");
                    Console.WriteLine("-IVC,--ignore-view-columns-directory <Path> - Path with ignore view columns file ignoreViewColumns.json");

                    return;
                }

                var @namespace = CommandLineHelper.GetParameterByName(args, "--namespace")
                                 ?? CommandLineHelper.GetParameterByName(args, "-N")
                                 ?? defaultNamespace;

                var contextName = CommandLineHelper.GetParameterByName(args, "--context")
                                  ?? CommandLineHelper.GetParameterByName(args, "-C")
                                  ?? defaultContext;

                var models = CommandLineHelper.GetParameterByName(args, "--models")
                                  ?? CommandLineHelper.GetParameterByName(args, "-M")
                                  ?? defaultModels;

                var schemas = CommandLineHelper.GetParameterByName(args, "--schema")
                             ?? CommandLineHelper.GetParameterByName(args, "-S")
                             ?? string.Empty;

                var excludeTables = CommandLineHelper.GetParameterByName(args, "--ignore-tables")
                              ?? CommandLineHelper.GetParameterByName(args, "-IT")
                              ?? string.Empty;

                var excludeTableColumnsDirectory = CommandLineHelper.GetParameterByName(args, "--ignore-table-columns-directory")
                                        ?? CommandLineHelper.GetParameterByName(args, "-ITC")
                                        ?? string.Empty;

                var excludeViews = CommandLineHelper.GetParameterByName(args, "--ignore-views")
                                    ?? CommandLineHelper.GetParameterByName(args, "-IV")
                                    ?? string.Empty;

                var excludeViewColumnsDirectory = CommandLineHelper.GetParameterByName(args, "--ignore-view-columns-directory")
                                         ?? CommandLineHelper.GetParameterByName(args, "-IVC")
                                         ?? string.Empty;

                var cleanUp = (CommandLineHelper.GetParameterByName(args, "--clean-up")
                                    ?? CommandLineHelper.GetParameterByName(args, "-CU")) != null;

                var generateStoredProcedures = CommandLineHelper.HasParameterByName(args, "--stored-procedures")
                               || CommandLineHelper.HasParameterByName(args, "-SP");

                var ignoreStoredProcedures = CommandLineHelper.GetParameterByName(args, "--ignore-stored-procedures")
                                       ?? CommandLineHelper.GetParameterByName(args, "-ISP")
                                       ?? string.Empty;

                var generateTableValueFunctions = CommandLineHelper.HasParameterByName(args, "--table-valued-functions")
                                               || CommandLineHelper.HasParameterByName(args, "-TVF");

                var ignoreTableValuedFunctions = CommandLineHelper.GetParameterByName(args, "--ignore-table-valued-functions")
                                             ?? CommandLineHelper.GetParameterByName(args, "-ITVF")
                                             ?? string.Empty;

                var fkPropertyRegex = CommandLineHelper.GetParameterByName(args, "--foreign-key-regex")
                                       ?? CommandLineHelper.GetParameterByName(args, "-FR")
                                       ?? string.Empty;

                var connectionString = CommandLineHelper.GetParameterByName(args, "--connectionString")
                                       ?? CommandLineHelper.GetParameterByName(args, "-SC")
                                       ?? string.Empty;

                var templatesDirectory = CommandLineHelper.GetParameterByName(args, "--templates-directory")
                                       ?? CommandLineHelper.GetParameterByName(args, "-TD")
                                       ?? Environment.CurrentDirectory;

                var extendedPropertyTypeName = CommandLineHelper.GetParameterByName(args, "--extended-property-type-name")
                                       ?? CommandLineHelper.GetParameterByName(args, "-ETN")
                                       ?? "TypeName";

                var isVerbose = CommandLineHelper.HasParameterByName(args, "--verbose")
                                || CommandLineHelper.HasParameterByName(args, "-V");

                var foreignKeysPresetDirectory = CommandLineHelper.GetParameterByName(args, "--foreign-keys-preset-directory")
                                       ?? CommandLineHelper.GetParameterByName(args, "-FKPD")
                                       ?? string.Empty;

                var includeSchemas = schemas.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();
                var excludeTablesList = excludeTables.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();
                var excludeViewsList = excludeViews.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();
                var excludeStoredProceduresList = ignoreStoredProcedures.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();
                var excludeTableValuedFunctionsList = ignoreTableValuedFunctions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();

                var directory = Environment.CurrentDirectory;

                var options = new ScaffoldOptions
                {
                    ConnectionString = connectionString,
                    Directory = directory,
                    TemplatesDirectory = templatesDirectory,
                    ModelsPath = models,
                    Schemas = includeSchemas,
                    IgnoreTables = excludeTablesList,
                    IgnoreViews = excludeViewsList,
                    GenerateStoredProcedures = generateStoredProcedures,
                    IgnoreStoredProcedure = excludeStoredProceduresList,
                    GenerateTableValuedFunctions = generateTableValueFunctions,
                    IgnoreTableValuedFunctions = excludeTableValuedFunctionsList,
                    Namespace = @namespace,
                    ForeignPropertyRegex = fkPropertyRegex,
                    ContextName = contextName,
                    IsVerbose = isVerbose,
                    ExtendedPropertyTypeName = extendedPropertyTypeName,
                    CleanUp = cleanUp,
                    ForeignKeysPresetDirectory = foreignKeysPresetDirectory,
                    ExcludeTableColumnsDirectory = excludeTableColumnsDirectory,
                    ExcludeViewColumnsDirectory = excludeViewColumnsDirectory
                };
                var scaffolder = new Scaffolder(options);
                scaffolder.Generate();
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds} ms.");
                ConsoleHelper.ConsoleReadLineWithTimeout(TimeSpan.FromSeconds(10));
            }
        }
    }
}
