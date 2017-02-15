using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Linq;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    public class IgniteLinqBenchmark
    {
        private const int PersonCount = 100;

        private const int SelectCount = PersonCount / 2;

        private readonly ICache<int, SqlPerson> _cache;

        private readonly SqlFieldsQuery _sqlQuery;

        private readonly IQueryable<int> _linq;

        private readonly Func<IQueryCursor<int>> _compiledLinq;

        public IgniteLinqBenchmark()
        {
            var ignite = Ignition.TryGetIgnite()
                         ?? Ignition.Start(new IgniteConfiguration
                         {
                             BinaryConfiguration = new BinaryConfiguration(typeof(SqlPerson)),
                             CacheConfiguration = new[] {new CacheConfiguration("persons", typeof(SqlPerson))}
                         });

            _cache = ignite.GetCache<int, SqlPerson>("persons");

            _cache.PutAll(Enumerable.Range(0, PersonCount)
                .ToDictionary(x => x, x => new SqlPerson {Id = x, Age = x * 2}));

            // Prepare queries.
            _sqlQuery = new SqlFieldsQuery("select Age from SqlPerson where (SqlPerson.Id < ?)", SelectCount);

            var persons = _cache.AsCacheQueryable();

            _linq = persons.Where(x => x.Value.Id < SelectCount).Select(x => x.Value.Age);

            _compiledLinq = CompiledQuery2.Compile(() => persons
                .Where(x => x.Value.Id < SelectCount).Select(x => x.Value.Age));
        }

        [Benchmark]
        public void QuerySql()
        {
            var res = _cache.QueryFields(_sqlQuery).GetAll();

            CheckResults(res.Select(x => (int) x[0]).ToList());
        }

        [Benchmark]
        public void QueryLinq()
        {
            var res = _linq.ToList();

            CheckResults(res);
        }

        [Benchmark]
        public void QueryLinqCompiled()
        {
            var res = _compiledLinq().GetAll();

            CheckResults(res);
        }

        private static void CheckResults(IList<int> ages)
        {
            if (ages.Count != SelectCount)
                throw new Exception("Invalid result");

            for (int i = 0; i < ages.Count; i++)
            {
                if (ages[i] != i * 2)
                    throw new Exception("Invalid result");
            }
        }

        private class SqlPerson
        {
            [QuerySqlField]
            public int Id { get; set; }

            [QuerySqlField]
            public int Age{ get; set; }
        }
    }
}
