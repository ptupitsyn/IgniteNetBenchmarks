using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace IgniteNetBenchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporterAttribute.GitHub]
    [MemoryDiagnoser]
    public class PlatformCacheBenchmark
    {
        private IIgnite _ignite;

        private ICache<int, Person> _cache;

        private ICache<int, Person> _cacheWithPlatformCache;

        [Benchmark]
        public void CacheGet()
        {
            var res = _cache.Get(1);

            if (res.Id != 42)
                throw new Exception();
        }

        [Benchmark(Baseline = true)]
        public void CacheGetWithPlatform()
        {
            var res = _cacheWithPlatformCache.Get(1);

            if (res.Id != 42)
                throw new Exception();
        }

        [Benchmark]
        public void CacheQueryScan()
        {
            var res = _cache.Query(new ScanQuery<int, Person>(new Filter())).Single().Value;

            if (res.Id != 42)
                throw new Exception();
        }

        [Benchmark(Baseline = true)]
        public void CacheQueryScanWithPlatform()
        {
            var res = _cacheWithPlatformCache.Query(new ScanQuery<int, Person>(new Filter())).Single().Value;

            if (res.Id != 42)
                throw new Exception();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _ignite = Ignition.Start();

            _cache = _ignite.CreateCache<int, Person>("normalCache");

            _cacheWithPlatformCache = _ignite.CreateCache<int, Person>(new CacheConfiguration
            {
                Name = "platformEnabledCache",
                PlatformCacheConfiguration = new PlatformCacheConfiguration
                {
                    KeyTypeName = typeof(int).AssemblyQualifiedName,
                    ValueTypeName = typeof(Person).AssemblyQualifiedName
                }
            });

            var data = Enumerable.Range(1, 10000)
                .Select(Person.CreateInstance<Person>)
                .Select(p => new KeyValuePair<int, Person>(p.Id, p))
                .ToArray();

            _cache.PutAll(data);
            _cacheWithPlatformCache.PutAll(data);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _ignite.Dispose();
        }

        private class Filter : ICacheEntryFilter<int, Person>
        {
            public bool Invoke(ICacheEntry<int, Person> entry)
            {
                return entry.Key == 42;
            }
        }
    }
}
