using System;
using System.Linq;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class CloneHelper
    {
        public static TOut CloneCopy<TIn, TOut>(this TIn source)
            where TOut : class, new()
            where TIn : class
        {
            if (!typeof(TOut).IsSubclassOf(typeof(TIn)))
                throw new InvalidOperationException($"Can't assign {typeof(TOut).FullName} from {typeof(TIn).FullName}");

            var result = new TOut();

            var publicProps = typeof(TIn).GetProperties().Where(x => x.CanRead && x.CanWrite);

            foreach (var publicProp in publicProps)
            {
                var value = publicProp.GetValue(source);
                publicProp.SetValue(result, value);
            }

            return result;
        }
    }
}