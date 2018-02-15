namespace EfCoreScaffoldMssql.Helpers
{
    public class CommandLineHelper
    {
        public static string GetParameterByName(string[] args, string name)
        {
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name && args.Length > i - 1)
                    return args[i + 1];
            }

            return null;
        }

        public static bool HasParameterByName(string[] args, string name)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == name)
                    return true;
            }

            return false;
        }
    }
}