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
    //[Config("jobs=LongRun")]
    public class SqlBenchmark
    {
        private readonly ICache<int, Person> _cache = Setup();

        //[Setup]
        public static ICache<int, Person> Setup()
        {
            Ignition.StopAll(true);

            var ignite = Ignition.Start(new IgniteConfiguration
            {
                BinaryConfiguration = new BinaryConfiguration(typeof (Person))
            });

            var cache = ignite.CreateCache<int, Person>(new CacheConfiguration("persons", typeof(Person)));

            cache.PutAll(Enumerable.Range(1, 10000).ToDictionary(x => x, x => new Person
            {
                Id = x,
                Data = Guid.NewGuid().ToString(),
                Name = "Vasya Petin " + x
            }));

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
