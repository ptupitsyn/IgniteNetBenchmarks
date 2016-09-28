using System;
using System.Linq;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using BenchmarkDotNet.Attributes;

namespace IgniteNetBenchmarks
{
    public class IgniteSerializationBenchmark
    {
        private readonly Func<object, byte[]> _serialize;

        private readonly Func<byte[], bool, object> _deserialize;

        private readonly Person _person = new Person
        {
            Id = 65535,
            Name = "John Johnson",
            Data = Enumerable.Range(1, 10).Select(x => Guid.NewGuid().ToString()).Aggregate((x, y) => x + y)
        };

        public IgniteSerializationBenchmark()
        {
            var ignite = Ignition.TryGetIgnite()
                         ?? Ignition.Start(new IgniteConfiguration
                         {
                             BinaryConfiguration = new BinaryConfiguration(typeof(Person))
                         });

            var marsh =
                ignite.GetType()
                    .GetProperty("Marshaller", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(ignite, null);

            var marshalMethod = marsh.GetType().GetMethods()
                .Single(x => x.Name == "Marshal" && x.GetParameters().Length == 1).MakeGenericMethod(typeof(object));

            _serialize =
                (Func<object, byte[]>) Delegate.CreateDelegate(typeof(Func<object, byte[]>), marsh, marshalMethod);

            var unmarshalMethod =
                marsh.GetType()
                    .GetMethod("Unmarshal", new[] {typeof(byte[]), typeof(bool)})
                    .MakeGenericMethod(typeof(object));
            _deserialize =
                (Func<byte[], bool, object>)
                Delegate.CreateDelegate(typeof(Func<byte[], bool, object>), marsh, unmarshalMethod);
        }

        [Benchmark]
        public void IgniteReflective()
        {
            var bytes = _serialize(_person);
            var result = (Person)_deserialize(bytes, false);

            if (_person.Data != result.Data)
                throw new Exception();
        }

        //[Benchmark]
        public void IgniteReflectiveRaw()
        {
            // TODO
        }
    }
}