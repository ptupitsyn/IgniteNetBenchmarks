using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    public class SqlServerBenchmark
    {
        private readonly SqlCommand _sqlCommand = SetupSql();

        private static SqlCommand SetupSql()
        {
            SqlDb.ResetPersons();

            var cmd = new SqlCommand("select Id from [IgniteNetBenchmarks].[dbo].Persons where ID > @min and ID < @max",
                SqlDb.GetOpenConnection());

            cmd.Parameters.Add("@min", SqlDbType.Int).Value = SqlDb.IdMin;
            cmd.Parameters.Add("@max", SqlDbType.Int).Value = SqlDb.IdMax;

            cmd.Prepare();

            return cmd;
        }

        [Benchmark]
        public void SqlServer()
        {
            using (var reader = _sqlCommand.ExecuteReader())
            {
                var persons = ReadPersons(reader).ToList();

                if (persons.Count != SqlDb.IdMax - SqlDb.IdMin - 1)
                    throw new Exception();
            }
        }

        private static IEnumerable<Person> ReadPersons(IDataReader reader)
        {
            while (reader.Read())
            {
                yield return new Person
                {
                    Id = reader.GetInt32(0),
                    //Name = reader.GetString(1),
                    //Data = reader.GetString(2)
                };
            }
        }
    }
}
