using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IgniteNetBenchmarks
{
    public static class SqlDb
    {
        public const string SqlConnectionString =
            @"Integrated Security=SSPI;Persist Security Info=False;Data Source=.\SQLEXPRESS";

        public static SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(SqlConnectionString);
            conn.Open();
            return conn;
        }

        public static void ResetPersons()
        {
            using (var conn = GetOpenConnection())
            {
                foreach (var statement in SplitSqlStatements(File.ReadAllText("Persons.sql")))
                {
                    new SqlCommand(statement, conn).ExecuteNonQuery();
                }
            }

            PopulatePersons();
        }

        private static void PopulatePersons()
        {
            using (var conn = GetOpenConnection())
            {
                foreach (var person in GetTestData())
                {
                    var cmd = new SqlCommand("insert into [IgniteNetBenchmarks].[dbo].Persons  values (@name, @data)", conn);

                    cmd.Parameters.AddWithValue("@name", person.Name);
                    cmd.Parameters.AddWithValue("@data", person.Data);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static IEnumerable<Person> GetTestData()
        {
            return Enumerable.Range(1, 10000).Select(x => new Person
            {
                Id = x,
                Data = Guid.NewGuid().ToString(),
                Name = "Vasya Petin " + x
            });
        }

        private static IEnumerable<string> SplitSqlStatements(string sqlScript)
        {
            // Split by "GO" statements
            var statements = Regex.Split(
                    sqlScript,
                    @"^\s*GO\s* ($ | \-\- .*$)",
                    RegexOptions.Multiline |
                    RegexOptions.IgnorePatternWhitespace |
                    RegexOptions.IgnoreCase);

            // Remove empties, trim, and return
            return statements
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim(' ', '\r', '\n'));
        }
    }
}