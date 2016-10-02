namespace EventSourcing.Library.Persistence
{
    public class DataWithVersion
    {
        public byte[] Data { get; private set; }
        public int Version { get; private set; }

        public DataWithVersion(int version, byte[]data)
        {
            this.Version = version;
            this.Data = data;
        }
    }
}