using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WallChartExample.Models;

namespace WallChartExample
{
    public class SQLReader
    {
                
        private readonly string connectionString;
        private readonly string schema;

        private readonly NpgsqlConnection connection;

        public SQLReader(string connectionString, string schema)
        {
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
            new NpgsqlCommand($"SET search_path = {schema};", connection).ExecuteNonQuery();

            this.connectionString = connectionString;
            this.schema = schema;
        }

        public IQueryable<T> RunQuery<T>(string sql, Func<NpgsqlDataReader, T> resultSelector)
        {        
            var result = new List<T>();         

            using (var cmd = new NpgsqlCommand(sql, connection))
            {

                using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        result.Add(resultSelector(rdr));
                    }
                }
            }


            return result.AsQueryable();
        }
    }
}
