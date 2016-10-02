using System.Collections.Generic;

namespace EventSourcing.Library.Persistence
{
    public interface IAppendOnlyStore
    {
        void Initialize(Dictionary<string, string> configuration);
        void Append(string name, byte[] data, int expectedVersion = -1);
        IEnumerable<DataWithVersion> ReadRecords(string name, long afterVersion, int maxCount);
        IEnumerable<DataWithName> ReadRecords(long afterVersion, int maxCount);
    }
}