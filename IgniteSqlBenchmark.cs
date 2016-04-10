using System;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    public class IgniteSqlBenchmark
    {
        private readonly ICache<int, Person> _cache = SetupIgnite();

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
            var sqlQuery = new SqlQuery(typeof (Person), "where id > ? and id < ?", 1000, 1100);
            var res = _cache.Query(sqlQuery).GetAll();

            if (res.Count != 9)
                throw new Exception();
        }
    }
}
