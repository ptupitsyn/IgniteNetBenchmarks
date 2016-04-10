using Apache.Ignite.Core.Cache.Configuration;

namespace IgniteNetBenchmarks
{
    public class Person
    {
        [QuerySqlField(IsIndexed = true)]
        public int Id { get; set; }

        [QuerySqlField]
        public string Name { get; set; }

        public string Data { get; set; }
    }
}