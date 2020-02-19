using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Discovery.Tcp;
using Apache.Ignite.Core.Discovery.Tcp.Static;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using ProtoBuf;
// ReSharper disable PossibleNullReferenceException

namespace IgniteNetBenchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp22)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class IgniteSerializationBenchmark
    {
        private readonly Func<object, byte[]> _serialize;

        private readonly Func<byte[], int, object> _deserialize;

        private readonly Person _person = Person.CreateInstance<Person>();

        private readonly PersonManual _personManual = Person.CreateInstance<PersonManual>();

        private readonly PersonSerializable _personSerializable = Person.CreateInstance<PersonSerializable>();

        private readonly PersonRaw _personRaw = Person.CreateInstance<PersonRaw>();

        private readonly PersonManualRaw _personManualRaw = Person.CreateInstance<PersonManualRaw>();

        public IgniteSerializationBenchmark()
        {
            var ignite = Ignition.TryGetIgnite() ?? Ignition.Start(GetIgniteConfiguration());

            var marsh =
                ignite.GetType()
                    .GetField("_marsh", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(ignite);

            var marshalMethod = marsh.GetType().GetMethods()
                .Single(x => x.Name == "Marshal" && x.GetParameters().Length == 1).MakeGenericMethod(typeof(object));

            _serialize =
                (Func<object, byte[]>) Delegate.CreateDelegate(typeof(Func<object, byte[]>), marsh, marshalMethod);

            var binaryMode = ignite.GetType().Assembly.GetType("Apache.Ignite.Core.Impl.Binary.BinaryMode");

            var unmarshalMethod =
                marsh.GetType()
                    .GetMethod("Unmarshal", new[] {typeof(byte[]), binaryMode})
                    .MakeGenericMethod(typeof(object));
            
            _deserialize =
                (Func<byte[], int, object>)
                Delegate.CreateDelegate(typeof(Func<byte[], int, object>), marsh, unmarshalMethod);
        }

        [Benchmark]
        public void IgniteReflective()
        {
            var bytes = _serialize(_person);
            var result = (Person)_deserialize(bytes, 0);

            if (!_person.IsEqual(result))
                throw new Exception();
        }

        [Benchmark]
        public void IgniteManual()
        {
            var bytes = _serialize(_personManual);
            var result = (PersonManual)_deserialize(bytes, 0);

            if (!_personManual.IsEqual(result))
                throw new Exception();
        }

        [Benchmark]
        public void IgniteSerializable()
        {
            var bytes = _serialize(_personSerializable);
            var result = (PersonSerializable)_deserialize(bytes, 0);

            if (!_personSerializable.IsEqual(result))
                throw new Exception();
        }

        [Benchmark]
        public void Protobuf()
        {
            var bytes = SerializeProtobuf(_person);
            var result = DeserializeProtobuf<Person>(bytes);

            if (!_person.IsEqual(result))
                throw new Exception();
        }

        [Benchmark]
        public void IgniteReflectiveRaw()
        {
            var bytes = _serialize(_personRaw);
            var result = (PersonRaw)_deserialize(bytes, 0);

            if (!_personRaw.IsEqual(result))
                throw new Exception();
        }

        [Benchmark]
        public void IgniteManualRaw()
        {
            var bytes = _serialize(_personManualRaw);
            var result = (PersonManualRaw)_deserialize(bytes, 0);

            if (!_personManualRaw.IsEqual(result))
                throw new Exception();
        }

        private static byte[] SerializeProtobuf(object obj)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);

                return ms.ToArray();
            }
        }

        private static T DeserializeProtobuf<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
        
        private static IgniteConfiguration GetIgniteConfiguration()
        {
            return new IgniteConfiguration
            {
                Localhost = "127.0.0.1",
                DiscoverySpi = new TcpDiscoverySpi
                {
                    IpFinder = new TcpDiscoveryStaticIpFinder
                    {
                        Endpoints = new[] { "127.0.0.1:47500" }
                    },
                    SocketTimeout = TimeSpan.FromSeconds(0.3)
                },
                BinaryConfiguration = new BinaryConfiguration(
                    typeof(Person), 
                    typeof(PersonManualRaw),
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
            };
        }
    }
}