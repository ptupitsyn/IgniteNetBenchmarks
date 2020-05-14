using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Client;
using Apache.Ignite.Core.Client.Cache;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    [BaselineColumn]
    [ShortRunJob]
    public class IgniteThinClientBenchmark
    {
        private ICacheClient<int, string> _cache;
        private ICacheClient<int, string> _cachePartitionAware;
        private ICache<int, string> _thickCache;

        [GlobalSetup]
        public void SetUp()
        {
            // This benchmark requires 3 server nodes started beforehand
            // docker run -d apacheignite/ignite
            var cfg = new IgniteClientConfiguration
            {
                Endpoints = new[]
                {
                    "172.17.0.2",
                    "172.17.0.3",
                    "172.17.0.4"
                }
            };
            
            var cfg2 = new IgniteClientConfiguration(cfg)
            {
                EnablePartitionAwareness = true
            };

            _cache = Ignition.StartClient(cfg).GetOrCreateCache<int, string>("c");
            _cachePartitionAware = Ignition.StartClient(cfg2).GetOrCreateCache<int, string>("c");

            _cache[1] = "Hello, World!";

            var thickClient = Ignition.Start(new IgniteConfiguration {ClientMode = true});
            _thickCache = thickClient.GetCache<int, string>("c");
        }

        [Benchmark]
        public void Get()
        {
            _cache.Get(1);
        }
        
        [Benchmark]
        public void GetPartitionAware()
        {
            _cachePartitionAware.Get(1);
        }

        [Benchmark]
        public void GetThick()
        {
            _thickCache.Get(1);
        }
    }
}