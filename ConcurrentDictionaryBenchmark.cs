using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    /// <summary>
    /// Compares generic vs non-generic <see cref="ConcurrentDictionary{TKey,TValue}"/> usage.
    /// For Native Near Cache feature.
    /// </summary>
    public class ConcurrentDictionaryBenchmark
    {
        private const int Count = 10000;

        private static readonly Guid[] Keys = Enumerable.Range(1, Count).Select(x => Guid.NewGuid()).ToArray();

        private static readonly int[] TestKeys = {25, 42, 2048, 8888};

        private static readonly ConcurrentDictionary<Guid, Person> GenericDict
            = new ConcurrentDictionary<Guid, Person>(GetData());

        private static readonly ConcurrentDictionary<object, object> ObjectDict
            = new ConcurrentDictionary<object, object>(GetData().Select(p =>
                new KeyValuePair<object, object>(p.Key, p.Value)));

        [Benchmark]
        public void TestGenericDict()
        {
            foreach (var keyIdx in TestKeys)
            {
                var key = Keys[keyIdx];
                var p = GenericDict[key];
                if (p.Id != keyIdx)
                {
                    throw new Exception("Bad result");
                }
            }
        }

        [Benchmark]
        public void TestObjectDict()
        {
            foreach (var keyIdx in TestKeys)
            {
                var key = Keys[keyIdx];
                var p = (Person) ObjectDict[key];
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
    }
}