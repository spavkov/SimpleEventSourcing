using System.Collections.Generic;
using EventSourcing.Library.Model;

namespace EventSourcing.Library.Persistence
{
    public interface IEventStore
    {
        EventStream LoadEventStream(IIdentity id);
        EventStream LoadEventStream(IIdentity id, long skip, int take);
        void AppendToStream(IIdentity id, int originalVersion, ICollection<IEvent> events);
    }
}