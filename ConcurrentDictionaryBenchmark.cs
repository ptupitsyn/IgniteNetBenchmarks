using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace IgniteNetBenchmarks
{
    /// <summary>
    /// Compares generic vs non-generic <see cref="ConcurrentDictionary{TKey,TValue}"/> usage.
    /// For Native Near Cache feature.
    /// </summary>
    [SimpleJob(RuntimeMoniker.NetCoreApp22)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    public class ConcurrentDictionaryBenchmark
    {
        private const int Count = 10000;

        private static readonly Guid[] Keys = Enumerable.Range(1, Count).Select(x => Guid.NewGuid()).ToArray();
        
        private static readonly string[] StringKeys = Keys.Select(k => k.ToString()).ToArray();

        private static readonly int[] TestKeys = {25, 42, 2048, 8888};

        private static readonly ConcurrentDictionary<Guid, Person> GenericDictGuidKeys
            = new ConcurrentDictionary<Guid, Person>(GetData());

        private static readonly ConcurrentDictionary<string, Person> GenericDictStringKeys
            = new ConcurrentDictionary<string, Person>(GetDataStringKeys());

        private static readonly ConcurrentDictionary<object, object> ObjectDictGuidKeys
            = new ConcurrentDictionary<object, object>(GetData().Select(p =>
                new KeyValuePair<object, object>(p.Key, p.Value)));

        private static readonly ConcurrentDictionary<object, object> ObjectDictStringKeys
            = new ConcurrentDictionary<object, object>(GetDataStringKeys().Select(p =>
                new KeyValuePair<object, object>(p.Key, p.Value)));

        [Benchmark]
        public void TestGenericDictGuidKeys()
        {
            foreach (var keyIdx in TestKeys)
            {
                var key = Keys[keyIdx];
                var p = GenericDictGuidKeys[key];
                if (p.Id != keyIdx)
                {
                    throw new Exception("Bad result");
                }
            }
        }

        [Benchmark]
        public void TestObjectDictGuidKeys()
        {
            foreach (var keyIdx in TestKeys)
            {
                var key = Keys[keyIdx];
                var p = (Person) ObjectDictGuidKeys[key];
                if (p.Id != keyIdx)
                {
                    throw new Exception("Bad result");
                }
            }
        }

        [Benchmark]
        public void TestGenericDictStringKeys()
        {
            foreach (var keyIdx in TestKeys)
            {
                var key = StringKeys[keyIdx];
                var p = GenericDictStringKeys[key];
                if (p.Id != keyIdx)
                {
                    throw new Exception("Bad result");
                }
            }
        }

        [Benchmark]
        public void TestObjectDictStringKeys()
        {
            foreach (var keyIdx in TestKeys)
            {
                var key = StringKeys[keyIdx];
                var p = (Person) ObjectDictStringKeys[key];
                if (p.Id != keyIdx)
                {
                    throw new Exception("Bad result");
                }
            }
        }

        private static IEnumerable<KeyValuePair<Guid, Person>> GetData()
        {
            return Keys
                .Select((k, i) => new KeyValuePair<Guid, Person>(k, Person.CreateInstance<Person>(i, 10)));
        }
        
        private static IEnumerable<KeyValuePair<string, Person>> GetDataStringKeys()
        {
            return StringKeys
                .Select((k, i) => new KeyValuePair<string, Person>(k, Person.CreateInstance<Person>(i, 10)));
        }
    }
}