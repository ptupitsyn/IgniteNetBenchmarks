using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using BenchmarkDotNet.Attributes;
using ProtoBuf;

namespace IgniteNetBenchmarks
{
    public class IgniteSerializationBenchmark
    {
        private readonly Func<object, byte[]> _serialize;

        private readonly Func<byte[], bool, object> _deserialize;

        private readonly Person _person = Person.CreateInstance<Person>();

        private readonly PersonManual _personManual = Person.CreateInstance<PersonManual>();

        private readonly PersonSerializable _personSerializable = Person.CreateInstance<PersonSerializable>();

        private readonly PersonRaw _personRaw = Person.CreateInstance<PersonRaw>();

        private readonly PersonManualRaw _personManualRaw = Person.CreateInstance<PersonManualRaw>();

        public IgniteSerializationBenchmark()
        {
            var ignite = Ignition.TryGetIgnite()
                         ?? Ignition.Start(new IgniteConfiguration
                         {
                             BinaryConfiguration = new BinaryConfiguration(typeof(Person), typeof(PersonManualRaw),
                                 typeof(PersonManual))
                             {
                                 TypeConfigurations =
                                 {
                                     new BinaryTypeConfiguration(typeof(PersonRaw))
                                     {
                                         Serializer = new BinaryReflectiveSerializer {RawMode = true}
                                     }
                                 }
                             }
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

        [Benchmark]
        public void IgniteManual()
        {
            var bytes = _serialize(_personManual);
            var result = (PersonManual)_deserialize(bytes, false);

            if (_personManual.Data != result.Data)
                throw new Exception();
        }

        [Benchmark]
        public void IgniteSerializable()
        {
            var bytes = _serialize(_personSerializable);
            var result = (PersonSerializable)_deserialize(bytes, false);

            if (_personSerializable.Data != result.Data)
                throw new Exception();
        }

        [Benchmark]
        public void Protobuf()
        {
            var bytes = SerializeProtobuf(_person);
            var result = DeserializeProtobuf<Person>(bytes);

            if (_person.Data != result.Data)
                throw new Exception();
        }

        [Benchmark]
        public void IgniteReflectiveRaw()
        {
            var bytes = _serialize(_personRaw);
            var result = (PersonRaw)_deserialize(bytes, false);

            if (_personRaw.Data != result.Data)
                throw new Exception();
        }

        [Benchmark]
        public void IgniteManualRaw()
        {
            var bytes = _serialize(_personManualRaw);
            var result = (PersonManualRaw)_deserialize(bytes, false);

            if (_personManualRaw.Data != result.Data)
                throw new Exception();
        }

        private static byte[] SerializeProtobuf(object obj)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);

                return ms.GetBuffer();
            }
        }

        private static T DeserializeProtobuf<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}