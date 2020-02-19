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
    public class IgniteSqlBenchmark
    {
        private readonly ICache<int, Person> _cache = SetupIgnite();

        private readonly Func<int, int, IQueryCursor<int>> _qry;

        private static readonly Person[] Data = SqlDb.GetTestData().ToArray();

        public IgniteSqlBenchmark()
        {
            var fieldsQuery = ((ICacheQueryable)_cache.AsCacheQueryable()
                .Select(x => x.Value.Id)
                .Where(x => x > SqlDb.IdMin && x < SqlDb.IdMax)).GetFieldsQuery();

            Console.WriteLine(fieldsQuery.Sql);

            var c = _cache.Query(
                new SqlFieldsQuery(
                    "select _T0.Id from \"persons\".Person as _T0 where ((_T0.Id > ?) and (_T0.Id < ?))", SqlDb.IdMin,
                    SqlDb.IdMax)).GetAll();

            Console.WriteLine(c.Count);

            Console.WriteLine(_cache.Query(fieldsQuery).GetAll().Count);

            _qry = CompiledQuery.Compile((int min, int max) => _cache.AsCacheQueryable()
                .Select(x => x.Value.Id)
                .Where(x => x > min && x < max));

            Console.WriteLine(_qry(SqlDb.IdMin, SqlDb.IdMax).GetAll().Count);
        }

        public static ICache<int, Person> SetupIgnite()
        {
            Ignition.StopAll(true);

            var ignite = Ignition.Start(new IgniteConfiguration
            {
                BinaryConfiguration = new BinaryConfiguration(typeof (Person))
            });

            var cache = ignite.CreateCache<int, Person>(new CacheConfiguration("persons", new QueryEntity(typeof(Person))));

            cache.PutAll(SqlDb.GetTestData().ToDictionary(x => x.Id, x => x));

            return cache;
        }

        [Benchmark]
        public void IgniteSql()
        {
            //var sqlQuery = new SqlFieldsQuery("select id from person where id > ? and id < ?", SqlDb.IdMin, SqlDb.IdMax);
            //var res = _cache.QueryFields(sqlQuery).GetAll();
            var res = _qry(SqlDb.IdMin, SqlDb.IdMax).GetAll();

            if (res.Count != SqlDb.IdMax - SqlDb.IdMin - 1)
                throw new Exception();
        }

        [Benchmark]
        public void RawArray()
        {
            var res = Data.Select(x => x.Id)
                .Where(x => x > SqlDb.IdMin && x < SqlDb.IdMax).ToList();

            if (res.Count != SqlDb.IdMax - SqlDb.IdMin - 1)
                throw new Exception();
        }
    }
}
