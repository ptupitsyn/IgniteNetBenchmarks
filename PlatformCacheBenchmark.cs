using System;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
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

            var person = Person.CreateInstance<Person>(42);
            _cache[1] = person;
            _cacheWithPlatformCache[1] = person;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _ignite.Dispose();
        }
    }
}
