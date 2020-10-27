using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity.Rendezvous;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Query;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Resource;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace IgniteNetBenchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporterAttribute.GitHub]
    [MemoryDiagnoser]
    public class PlatformCacheComputeBenchmark
    {
        private IIgnite _ignite;

        private ICache<int, Person> _cache;

        private ICache<int, Person> _cacheWithPlatformCache;

        [Benchmark]
        public void ComputeSum()
        {
            ComputeSum(_cache.Name);
        }

        [Benchmark(Baseline = true)]
        public void ComputeSumWithPlatform()
        {
            ComputeSum(_cacheWithPlatformCache.Name);
        }

        private void ComputeSum(string cacheName)
        {
            var partitions = _ignite.GetAffinity(cacheName).Partitions;
            var compute = _ignite.GetCompute();
            var cacheNames = new[] {cacheName};

            // Map: For every partition, perform an affinity call, which guarantees that specified partition stays
            // on the current node during the call.
            var res = Enumerable.Range(0, partitions)
                .Select(partition => compute.AffinityCall(
                    cacheNames,
                    partition,
                    new PersonDataSumFunc
                    {
                        CacheName = cacheName,
                        Partition = partition
                    }))
                // Reduce: Sum up the results.
                .Sum();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _ignite = Ignition.Start();

            // Reduce number of partitions to reduce overhead.
            // With real-world data sets this won't be needed.
            var affinityFunction = new RendezvousAffinityFunction
            {
                Partitions = 10
            };

            _cache = _ignite.CreateCache<int, Person>(new CacheConfiguration
            {
                Name = "normalCache",
                AffinityFunction = affinityFunction
            });

            _cacheWithPlatformCache = _ignite.CreateCache<int, Person>(new CacheConfiguration
            {
                Name = "platformEnabledCache",
                AffinityFunction = affinityFunction,
                PlatformCacheConfiguration = new PlatformCacheConfiguration
                {
                    KeyTypeName = typeof(int).AssemblyQualifiedName,
                    ValueTypeName = typeof(Person).AssemblyQualifiedName
                }
            });

            var data = Enumerable.Range(1, 100000)
                .Select(x => Person.CreateInstance<Person>(x, dataSize: 10))
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

        private class PersonDataSumFunc : IComputeFunc<long>
        {
            [InstanceResource]
            private IIgnite Ignite { get; set; }

            public string CacheName { get; set; }

            public int Partition { get; set; }

            public long Invoke()
            {
                var cache = Ignite.GetCache<int, Person>(CacheName);

                // Local partition scan iterates over Platform Cache entries directly.
                var query = new ScanQuery<int, Person>
                {
                    Local = true,
                    Partition = Partition
                };

                var sum = cache.Query(query)
                    .Select(entry => entry.Value.Data.Sum(c => (long) c))
                    .Sum();

                return sum;
            }
        }
    }
}
