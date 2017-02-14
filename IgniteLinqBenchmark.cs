using System;
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
        private const int PersonCount = 40;

        private const int SelectCount = PersonCount / 2;

        private readonly ICache<int, SqlPerson> _cache;

        private readonly SqlFieldsQuery _sqlQuery;

        private readonly IQueryable<string> _linq;

        private readonly Func<IQueryCursor<string>> _compiledLinq;

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
                .ToDictionary(x => x, x => new SqlPerson {Id = x, Name = "Person " + x}));

            // Prepare queries.
            _sqlQuery = new SqlFieldsQuery("select Name from SqlPerson where (SqlPerson.Id < ?)", SelectCount);

            var persons = _cache.AsCacheQueryable();

            _linq = persons.Where(x => x.Value.Id < SelectCount).Select(x => x.Value.Name);

            _compiledLinq = CompiledQuery2.Compile(() => persons
                .Where(x => x.Value.Id < SelectCount).Select(x => x.Value.Name));
        }

        [Benchmark]
        public void QuerySql()
        {
            var res = _cache.QueryFields(_sqlQuery).GetAll();

            if (res.Count != SelectCount)
                throw new Exception("Incorrect query result");
        }

        [Benchmark]
        public void QueryLinq()
        {
            var res = _linq.ToList();

            if (res.Count != SelectCount)
                throw new Exception("Incorrect query result");
        }

        [Benchmark]
        public void QueryLinqCompiled()
        {
            var res = _compiledLinq().GetAll();

            if (res.Count != SelectCount)
                throw new Exception("Incorrect query result");
        }

        private class SqlPerson
        {
            [QuerySqlField]
            public int Id { get; set; }

            [QuerySqlField]
            public string Name { get; set; }
        }
    }
}
