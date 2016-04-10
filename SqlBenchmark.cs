using System;
using System.Data.SqlClient;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    //[Config("jobs=LongRun")]
    public class SqlBenchmark
    {
        private readonly ICache<int, Person> _cache = SetupIgnite();

        private readonly SqlCommand _sqlCommand = SetupSql();

        private static SqlCommand SetupSql()
        {
            SqlDb.ResetPersons();

            var cmd = new SqlCommand("select * from [IgniteNetBenchmarks].[dbo].Persons where ID > @min and ID < @max",
                SqlDb.GetOpenConnection());

            cmd.Parameters.AddWithValue("@min", 1000);
            cmd.Parameters.AddWithValue("@max", 1010);

            cmd.Prepare();

            return cmd;
        }

        //[Setup]
        public static ICache<int, Person> SetupIgnite()
        {
            Ignition.StopAll(true);

            var ignite = Ignition.Start(new IgniteConfiguration
            {
                BinaryConfiguration = new BinaryConfiguration(typeof (Person))
            });

            var cache = ignite.CreateCache<int, Person>(new CacheConfiguration("persons", typeof(Person)));

            cache.PutAll(SqlDb.GetTestData().ToDictionary(x => x.Id, x => x));

            return cache;
        }

        [Benchmark]
        public void IgniteSql()
        {
            var sqlQuery = new SqlQuery(typeof (Person), "where id > ? and id < ?", 1000, 1010);
            var res = _cache.Query(sqlQuery).GetAll();

            if (res.Count != 9)
                throw new Exception();
        }

        [Benchmark]
        public void SqlServer()
        {
            using (var reader = _sqlCommand.ExecuteReader())
            {
                int i = 0;

                while (reader.Read())
                {
                    i++;
                }

                if (i != 9)
                    throw new Exception();
            }
        }
    }

    public class Person
    {
        [QuerySqlField(IsIndexed = true)]
        public int Id { get; set; }

        [QuerySqlField]
        public string Name { get; set; }

        public string Data { get; set; }
    }

}
