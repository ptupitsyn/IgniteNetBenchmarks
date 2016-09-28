using Apache.Ignite.Core.Cache.Configuration;
using ProtoBuf;

namespace IgniteNetBenchmarks
{
    [ProtoContract]
    public class Person
    {
        [ProtoMember(1)]
        [QuerySqlField(IsIndexed = true)]
        public int Id { get; set; }

        [ProtoMember(2)]
        [QuerySqlField]
        public string Name { get; set; }

        [ProtoMember(3)]
        [QuerySqlField]
        public string Data { get; set; }
    }
}