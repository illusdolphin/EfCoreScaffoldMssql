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
                    Console.WriteLine("-IT,--ignore-tables <Tables> - comma-separated list of tables to exclude. Example: '[dbo].Table1,[master].Table2'");
                    Console.WriteLine("-IV,--ignore-views <Views> - comma-separated list of view to exclude. Example: '[dbo].View1,[master].View2'");
                    Console.WriteLine("-FR,--foreign-key-regex <regex> - Regex to extract foreign property name. Example: 'FK_([a-zA-Z]+)_(?<PropertyName>.+)'");
                    Console.WriteLine("-CS,--connectionString <ConnectionString> - Connection string to the db, example: Data Source=.;Initial Catalog=Database;integrated security=SSPI");
                    Console.WriteLine("-TD,--templates-directory <Path> - Path with template fies set.hbs and context.hbs");
                    Console.WriteLine("-V,--verbose <Schema> - Show messages during execution");

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

                var excludeViews = CommandLineHelper.GetParameterByName(args, "--ignore-views")
                                    ?? CommandLineHelper.GetParameterByName(args, "-IV")
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

                var isVerbose = CommandLineHelper.HasParameterByName(args, "--verbose")
                                || CommandLineHelper.HasParameterByName(args, "-V");

                var includeSchemas = schemas.Split(new []{ ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();
                var excludeTablesList = excludeTables.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.ToLower())
                    .ToList();
                var excludeViewsList = excludeViews.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
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
                    Namespace = @namespace,
                    ForeignPropertyRegex = fkPropertyRegex,
                    ContextName = contextName,
                    IsVerbose = isVerbose
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
