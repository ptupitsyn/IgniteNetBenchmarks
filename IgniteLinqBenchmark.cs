using System;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Linq;

namespace IgniteNetBenchmarks
{
    public class IgniteLinqBenchmark
    {
        private readonly ICache<int, SqlPerson> _cache;

        public IgniteLinqBenchmark()
        {
            var ignite = Ignition.TryGetIgnite()
                         ?? Ignition.Start(new IgniteConfiguration
                         {
                             BinaryConfiguration = new BinaryConfiguration(typeof(SqlPerson)),
                             CacheConfiguration = new[] {new CacheConfiguration("persons", typeof(SqlPerson))}
                         });

            _cache = ignite.GetCache<int, SqlPerson>("persons");

            _cache.PutAll(Enumerable.Range(1, 100)
                .ToDictionary(x => x, x => new SqlPerson {Id = x, Name = "Person " + x}));
        }

        public void QuerySql()
        {
            var qry = new SqlFieldsQuery("select Name from SqlPerson where (SqlPerson.Id < ?)", 25);

            var res = _cache.QueryFields(qry).GetAll();

            if (res.Count != 24)
                throw new Exception("Incorrect query result");
        }

        public void QueryLinq()
        {
            var res = _cache.AsCacheQueryable()
                .Where(x => x.Value.Id < 25)
                .Select(x => x.Value.Name)
                .ToList();

            if (res.Count != 24)
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
