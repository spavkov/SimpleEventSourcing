namespace EventSourcing.Library.Persistence
{
    public class DataWithName
    {
        public byte[] Data { get; private set; }
        public string Name { get; private set; }

        public DataWithName(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }
    }
}