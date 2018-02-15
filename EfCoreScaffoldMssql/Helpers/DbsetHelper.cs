using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EfCoreScaffoldMssql.Helpers
{
    public static class DbsetHelper
    {
        public static List<T> ReadObjects<T>(this SqlConnection connection, string sql)
            where T : class, new()
        {
            var props = typeof(T).GetProperties()
                .Where(x => x.CanWrite)
                .ToList();

            using (var command = new SqlCommand(sql, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    var result = new List<T>();
                    while (reader.Read())
                    {
                        var newObj = new T();
                        foreach (var prop in props)
                        {
                            var value = reader[prop.Name];
                            var isNull = value is DBNull;
                            if (isNull)
                            {
                                value = null;
                            }
                            prop.SetValue(newObj, value);
                        }
                        result.Add(newObj);
                    }
                    return result;
                }
            }
        }

    }
}