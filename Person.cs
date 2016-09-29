using System;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache.Configuration;
using ProtoBuf;

namespace IgniteNetBenchmarks
{
    [ProtoContract]
    [Serializable]
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

    public class PersonRaw : Person
    {
        // No-op.
    }

    public class PersonManual : Person, IBinarizable
    {
        public void WriteBinary(IBinaryWriter writer)
        {
            writer.WriteInt("id", Id);
            writer.WriteString("name", Name);
            writer.WriteString("data", Data);
        }

        public void ReadBinary(IBinaryReader reader)
        {
            Id = reader.ReadInt("id");
            Name = reader.ReadString("name");
            Data = reader.ReadString("data");
        }
    }

    public class PersonManualRaw : Person, IBinarizable
    {
        public void WriteBinary(IBinaryWriter writer)
        {
            var raw = writer.GetRawWriter();

            raw.WriteInt(Id);
            raw.WriteString(Name);
            raw.WriteString(Data);
        }

        public void ReadBinary(IBinaryReader reader)
        {
            var raw = reader.GetRawReader();

            Id = raw.ReadInt();
            Name = raw.ReadString();
            Data = raw.ReadString();
        }
    }

    [Serializable]
    public class PersonSerializable : Person
    {
        // No-op.
    }
}