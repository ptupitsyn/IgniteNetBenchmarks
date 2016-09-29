using System;
using Apache.Ignite.Core.Binary;
using ProtoBuf;

namespace IgniteNetBenchmarks
{
    [ProtoContract]
    [Serializable]
    public class Person
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string Data { get; set; }

        [ProtoMember(4)]
        public Guid Guid { get; set; }

        public static T CreateInstance<T>() where T : Person, new()
        {
            return new T
            {
                Id = int.MinValue,
                Name = "John Johnson",
                Data = new string('g', 1000),
                Guid = Guid.NewGuid()
            };
        }

        public bool IsEqual(Person other)
        {
            return Id == other.Id && Name == other.Name && Data == other.Data && Guid == other.Guid;
        }
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
            writer.WriteGuid("guid", Guid);
        }

        public void ReadBinary(IBinaryReader reader)
        {
            Id = reader.ReadInt("id");
            Name = reader.ReadString("name");
            Data = reader.ReadString("data");
            Guid = reader.ReadGuid("guid").Value;
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
            raw.WriteGuid(Guid);
        }

        public void ReadBinary(IBinaryReader reader)
        {
            var raw = reader.GetRawReader();

            Id = raw.ReadInt();
            Name = raw.ReadString();
            Data = raw.ReadString();
            Guid = raw.ReadGuid().Value;
        }
    }

    [Serializable]
    public class PersonSerializable : Person
    {
        // No-op.
    }
}